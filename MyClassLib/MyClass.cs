using System.Text.Json;
using System.Text.Json.Nodes;
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
        });

        builder.Configure(app =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("string", ([FromQuery] string q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("string[]", ([FromQuery] string[] q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("int", ([FromQuery] int q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("int[]", ([FromQuery] int[] q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("bool", ([FromQuery] bool q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("double", ([FromQuery] double q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("float", ([FromQuery] float q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("byte", ([FromQuery] byte q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("byte[]", ([FromQuery] byte[] q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("sbyte", ([FromQuery] sbyte q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("char", ([FromQuery] char q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("decimal", ([FromQuery] decimal q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("uint", ([FromQuery] uint q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("long", ([FromQuery] long q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("ulong", ([FromQuery] ulong q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("short", ([FromQuery] short q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("ushort", ([FromQuery] ushort q) => JsonSerializer.Serialize(q));
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
                var responseContent = await response.Content.ReadAsStringAsync();
                return new(type, Result: JsonNode.Parse(responseContent));
            }
            catch (BadHttpRequestException e)
            {
                return new(type, Error: new("400 Bad Request", e.Message));
            }
        }

        return
        [
            await CheckBindingAsync("string"),
            await CheckBindingAsync("string[]"),
            await CheckBindingAsync("int"),
            await CheckBindingAsync("int[]"),
            await CheckBindingAsync("bool"),
            await CheckBindingAsync("double"),
            await CheckBindingAsync("float"),
            await CheckBindingAsync("byte"),
            await CheckBindingAsync("byte[]"),
            await CheckBindingAsync("sbyte"),
            await CheckBindingAsync("char"),
            await CheckBindingAsync("decimal"),
            await CheckBindingAsync("uint"),
            await CheckBindingAsync("long"),
            await CheckBindingAsync("ulong"),
            await CheckBindingAsync("short"),
            await CheckBindingAsync("ushort"),
        ];
    }
}
