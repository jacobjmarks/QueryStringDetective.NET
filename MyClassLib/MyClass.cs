using System.Net.Http.Json;
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
                endpoints.MapGet("string", ([FromQuery] string q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("string[]", ([FromQuery] string[] q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("int", ([FromQuery] int q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("int[]", ([FromQuery] int[] q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("bool", ([FromQuery] bool q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("double", ([FromQuery] double q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("float", ([FromQuery] float q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("byte", ([FromQuery] byte q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("byte[]", ([FromQuery] byte[] q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("sbyte", ([FromQuery] sbyte q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("char", ([FromQuery] char q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("decimal", ([FromQuery] decimal q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("uint", ([FromQuery] uint q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("long", ([FromQuery] long q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("ulong", ([FromQuery] ulong q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("short", ([FromQuery] short q, HttpResponse r) => r.WriteAsJsonAsync(q));
                endpoints.MapGet("ushort", ([FromQuery] ushort q, HttpResponse r) => r.WriteAsJsonAsync(q));
            });
        });

        return new TestServer(builder);
    }

    public static async Task<IEnumerable<BindingResult>> TestQueryStringBindingAsync(string queryString)
    {
        ArgumentNullException.ThrowIfNull(queryString);
        if (!queryString.StartsWith("?q="))
            throw new ArgumentException("Unexpected value.", nameof(queryString));

        using var minimalApiTestServer = CreateMinimalApiTestServer();
        using var minimalApiClient = minimalApiTestServer.CreateClient();

        async Task<BindingResult> CheckBindingAsync(string type, string? endpoint = null)
        {
            endpoint ??= type;

            try
            {
                using var response = await minimalApiClient.GetAsync(endpoint + queryString);
                response.EnsureSuccessStatusCode();
                return new(type, Result: await response.Content.ReadFromJsonAsync<JsonElement>());
            }
            catch (BadHttpRequestException e)
            {
                return new(type, Error: new("400 Bad Request", e.Message));
            }
        }

        return await Task.WhenAll(
        [
            CheckBindingAsync("string"),
            CheckBindingAsync("string[]"),
            CheckBindingAsync("int"),
            CheckBindingAsync("int[]"),
            CheckBindingAsync("bool"),
            CheckBindingAsync("double"),
            CheckBindingAsync("float"),
            CheckBindingAsync("byte"),
            CheckBindingAsync("byte[]"),
            CheckBindingAsync("sbyte"),
            CheckBindingAsync("char"),
            CheckBindingAsync("decimal"),
            CheckBindingAsync("uint"),
            CheckBindingAsync("long"),
            CheckBindingAsync("ulong"),
            CheckBindingAsync("short"),
            CheckBindingAsync("ushort"),
        ]);
    }
}
