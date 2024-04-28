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

    public QueryBindingEvaluator()
    {
        _minimalApiTestServer = CreateMinimalApiTestServer();
        _minimalApiTestServerClient = _minimalApiTestServer.CreateClient();
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
                foreach (var (route, @delegate) in Generated.RequestDelegates)
                    endpoints.MapGet(route, @delegate);
            });
        });

        return new TestServer(builder);
    }

    private async Task<BindingResult> GetMinimalApiBindingResult(EndpointDescriptor endpoint, string queryString)
    {
        try
        {
            using var response = await _minimalApiTestServerClient.GetAsync(endpoint.Route + queryString);
            response.EnsureSuccessStatusCode();
            return new(Result: await response.Content.ReadAsStringAsync());
        }
        catch (BadHttpRequestException e)
        {
            return new(Error: new("400 Bad Request", e.Message));
        }
    }

    private async Task<BindingResults> GetBindingResultsAsync(EndpointDescriptor endpoint, string queryString)
    {
        return new BindingResults(endpoint.Type, new()
        {
             { ApiType.MinimalApis, await GetMinimalApiBindingResult(endpoint, queryString) },
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
        GC.SuppressFinalize(this);
    }
}
