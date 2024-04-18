using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using MySharedClassLib;

namespace MyClassLib;

public static class MyClass
{
    private static readonly (string Route, Type ParamType)[] _endpoints =
    [
        ("string", typeof(string)),
        ("string[]", typeof(string[])),
        ("char", typeof(char)),
        ("char[]", typeof(char[])),
        ("int", typeof(int)),
        ("int[]", typeof(int[])),
        ("double", typeof(double)),
        ("double[]", typeof(double[])),
        ("float", typeof(float)),
        ("float[]", typeof(float[])),
        ("decimal", typeof(decimal)),
        ("decimal[]", typeof(decimal[])),
        ("bool", typeof(bool)),
        ("bool[]", typeof(bool[])),
        ("uint", typeof(uint)),
        ("uint[]", typeof(uint[])),
        ("long", typeof(long)),
        ("long[]", typeof(long[])),
        ("ulong", typeof(ulong)),
        ("ulong[]", typeof(ulong[])),
        ("sbyte", typeof(sbyte)),
        ("sbyte[]", typeof(sbyte[])),
        ("short", typeof(short)),
        ("short[]", typeof(short[])),
        ("ushort", typeof(ushort)),
        ("ushort[]", typeof(ushort[])),
        ("byte", typeof(byte)),
        ("byte[]", typeof(byte[])),
    ];

    private static Delegate CreateEndpointDelegate<T>() => ([FromQuery] T q, HttpResponse r) => r.WriteAsJsonAsync(q);
    private static readonly MethodInfo createEndpointDelegateMethod = typeof(MyClass)
        .GetMethod(nameof(CreateEndpointDelegate), 1, BindingFlags.NonPublic | BindingFlags.Static, null, [], null)
            ?? throw new InvalidOperationException($"Could not find {nameof(CreateEndpointDelegate)} method.");

    private static readonly TestServer _minimalApiTestServer = CreateMinimalApiTestServer();
    private static readonly HttpClient _minimalApiTestServerClient = _minimalApiTestServer.CreateClient();

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

    private static async Task<BindingResult> GetBindingResultAsync((string Route, Type _) endpoint, string queryString)
    {
        try
        {
            using var response = await _minimalApiTestServerClient.GetAsync(endpoint.Route + queryString);
            response.EnsureSuccessStatusCode();
            return new BindingResult(endpoint.Route, Result: await response.Content.ReadFromJsonAsync<JsonElement>());
        }
        catch (BadHttpRequestException e)
        {
            return new BindingResult(endpoint.Route, Error: new("400 Bad Request", e.Message));
        }
    }

    public static async Task<IEnumerable<BindingResult>> TestQueryStringBindingAsync(string queryString)
    {
        ArgumentNullException.ThrowIfNull(queryString);
        if (!queryString.StartsWith("?q="))
            throw new ArgumentException("Unexpected value.", nameof(queryString));

        var bindingResults = _endpoints.Select(endpoint => GetBindingResultAsync(endpoint, queryString));
        return await Task.WhenAll(bindingResults);
    }

    public static void Dispose()
    {
        _minimalApiTestServerClient.Dispose();
        _minimalApiTestServer.Dispose();
    }
}
