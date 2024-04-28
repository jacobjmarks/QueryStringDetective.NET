using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Server.Core;

public class QueryBindingEvaluator : IDisposable
{
    private readonly TestServer _minimalApiTestServer;
    private readonly HttpClient _minimalApiTestServerClient;

    private readonly TestServer _controllerTestServer;
    private readonly HttpClient _controllerTestServerClient;

    public QueryBindingEvaluator()
    {
        _minimalApiTestServer = CreateMinimalApiTestServer();
        _minimalApiTestServerClient = _minimalApiTestServer.CreateClient();

        _controllerTestServer = CreateControllerTestServer();
        _controllerTestServerClient = _controllerTestServer.CreateClient();
    }

    private static TestServer CreateMinimalApiTestServer()
    {
        var builder = new WebHostBuilder();

        builder.ConfigureServices(services =>
        {
            services.AddRouting();
            services.Configure<RouteHandlerOptions>(o => o.ThrowOnBadRequest = true);
            services.ConfigureHttpJsonOptions(json =>
            {
                json.SerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
            });
        });

        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                foreach (var (route, @delegate) in Generated.MinimalApiRequestDelegates)
                    endpoints.MapGet(route, @delegate);
            });
        });

        return new TestServer(builder);
    }

    private static TestServer CreateControllerTestServer()
    {
        var builder = new WebHostBuilder();

        builder.ConfigureServices(services =>
        {
            services.AddMvcCore()
                .AddApplicationPart(typeof(QueryBindingEvaluator).Assembly)
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                });
        });

        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        });

        return new TestServer(builder);
    }

    private async Task<BindingResult> GetBindingResult(ApiType apiType, EndpointDescriptor endpoint, string queryString)
    {
        var httpClient = apiType switch
        {
            ApiType.MinimalApis => _minimalApiTestServerClient,
            ApiType.Controllers => _controllerTestServerClient,
            _ => throw new NotSupportedException(),
        };

        try
        {
            using var response = await httpClient.GetAsync(endpoint.Route + queryString);
            response.EnsureSuccessStatusCode();
            return new(Result: await response.Content.ReadAsStringAsync());
        }
        catch (BadHttpRequestException e)
        {
            return new(Error: new("400 Bad Request", e.Message));
        }
        catch (HttpRequestException e)
        {
            return new(Error: new(e.StatusCode.ToString()!, e.Message));
        }
    }

    private async Task<BindingResults> GetBindingResultsAsync(EndpointDescriptor endpoint, string queryString)
    {
        return new BindingResults(endpoint.Type, new()
        {
             { ApiType.MinimalApis, await GetBindingResult(ApiType.MinimalApis, endpoint, queryString) },
             { ApiType.Controllers, await GetBindingResult(ApiType.Controllers, endpoint, queryString) },
        });
    }

    public async Task<IEnumerable<BindingResults>> EvaluateAsync(string queryString)
    {
        ArgumentNullException.ThrowIfNull(queryString);
        if (!queryString.StartsWith("?q="))
            throw new ArgumentException("Unexpected value.", nameof(queryString));

        var bindingResults = Constants.Endpoints.Select(endpoint => GetBindingResultsAsync(endpoint, queryString));
        return await Task.WhenAll(bindingResults);
    }

    public void Dispose()
    {
        _minimalApiTestServerClient.Dispose();
        _minimalApiTestServer.Dispose();

        _controllerTestServerClient.Dispose();
        _controllerTestServer.Dispose();

        GC.SuppressFinalize(this);
    }
}
