using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

namespace Server.Core;

public sealed class QueryBindingEvaluator : IDisposable
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

        builder.UseEnvironment(Environments.Production);

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

        builder.UseEnvironment(Environments.Production);

        builder.ConfigureServices(services =>
        {
            services.AddMvcCore()
                .AddApplicationPart(typeof(QueryBindingEvaluator).Assembly)
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                });

            services.AddProblemDetails();
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

        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync(endpoint.Route + queryString);
        }
        catch (BadHttpRequestException e)
        {
            return new(Error: new("400 Bad Request", e.Message));
        }

        try
        {
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                var reasonPhrase = ReasonPhrases.GetReasonPhrase(statusCode);

                string? detail;
                try
                {
                    var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                    detail = problemDetails?.Errors["q"][0];
                }
                catch
                {
                    detail = null;
                }

                return new(Error: new($"{statusCode} {reasonPhrase}", detail));
            }

            return new(Result: await response.Content.ReadAsStringAsync());
        }
        finally
        {
            response.Dispose();
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
