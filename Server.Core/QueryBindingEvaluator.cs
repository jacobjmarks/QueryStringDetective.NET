using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Shared;

namespace Server.Core;

public class QueryBindingEvaluator : IDisposable
{
    private static readonly EndpointDescriptor[] _endpoints =
    [
        new("string", typeof(string)),
        new("string[]", typeof(string[])),
        new("char", typeof(char)),
        new("char[]", typeof(char[])),
        new("int", typeof(int)),
        new("int[]", typeof(int[])),
        new("double", typeof(double)),
        new("double[]", typeof(double[])),
        new("float", typeof(float)),
        new("float[]", typeof(float[])),
        new("decimal", typeof(decimal)),
        new("decimal[]", typeof(decimal[])),
        new("bool", typeof(bool)),
        new("bool[]", typeof(bool[])),
        new("uint", typeof(uint)),
        new("uint[]", typeof(uint[])),
        new("long", typeof(long)),
        new("long[]", typeof(long[])),
        new("ulong", typeof(ulong)),
        new("ulong[]", typeof(ulong[])),
        new("sbyte", typeof(sbyte)),
        new("sbyte[]", typeof(sbyte[])),
        new("short", typeof(short)),
        new("short[]", typeof(short[])),
        new("ushort", typeof(ushort)),
        new("ushort[]", typeof(ushort[])),
        new("byte", typeof(byte)),
        new("byte[]", typeof(byte[])),
    ];

    private static Delegate CreateEndpointDelegate<T>() => ([FromQuery] T q, HttpResponse r) => r.WriteAsJsonAsync(q);
    private static readonly MethodInfo createEndpointDelegateMethod = typeof(QueryBindingEvaluator)
        .GetMethod(nameof(CreateEndpointDelegate), 1, BindingFlags.NonPublic | BindingFlags.Static, null, [], null)
            ?? throw new InvalidOperationException($"Could not find {nameof(CreateEndpointDelegate)} method.");

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
                foreach (var (route, paramType) in _endpoints)
                    endpoints.MapGet(route, (Delegate)createEndpointDelegateMethod.MakeGenericMethod(paramType).Invoke(null, null)!);
            });
        });

        return new TestServer(builder);
    }

    private async Task<BindingResult> GetBindingResultAsync(EndpointDescriptor endpoint, string queryString)
    {
        try
        {
            using var response = await _minimalApiTestServerClient.GetAsync(endpoint.Route + queryString);
            response.EnsureSuccessStatusCode();
            return new BindingResult(endpoint.Route, Result: await response.Content.ReadAsStringAsync());
        }
        catch (BadHttpRequestException e)
        {
            return new BindingResult(endpoint.Route, Error: new("400 Bad Request", e.Message));
        }
    }

    public async Task<IEnumerable<BindingResult>> EvaluateAsync(string queryString)
    {
        ArgumentNullException.ThrowIfNull(queryString);
        if (!queryString.StartsWith("?q="))
            throw new ArgumentException("Unexpected value.", nameof(queryString));

        var bindingResults = _endpoints.Select(endpoint => GetBindingResultAsync(endpoint, queryString));
        return await Task.WhenAll(bindingResults);
    }

    public void Dispose()
    {
        _minimalApiTestServerClient.Dispose();
        _minimalApiTestServer.Dispose();
        GC.SuppressFinalize(this);
    }
}
