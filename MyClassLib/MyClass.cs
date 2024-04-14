using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace MyClassLib;

public static class MyClass
{
    private static TestServer CreateMinimalApiTestServer()
    {
        var builder = new WebHostBuilder();

        builder.ConfigureServices(services =>
        {
            services.AddRouting();
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

    public static async Task<JsonObject> TestQueryStringBindingAsync(string queryString)
    {
        ArgumentNullException.ThrowIfNull(queryString);
        if (!queryString.StartsWith("?q="))
            throw new ArgumentException("Unexpected value.", nameof(queryString));

        using var minimalApiTestServer = CreateMinimalApiTestServer();
        using var minimalApiClient = minimalApiTestServer.CreateClient();

        async Task<JsonNode?> CheckBindingAsync(string endpoint)
        {
            using var response = await minimalApiClient.GetAsync(endpoint + queryString);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonNode.Parse(responseContent);
        }

        var result = new JsonObject();
        await TryAddResult(result, "string", CheckBindingAsync);
        await TryAddResult(result, "string[]", CheckBindingAsync);
        await TryAddResult(result, "int", CheckBindingAsync);
        await TryAddResult(result, "int[]", CheckBindingAsync);
        await TryAddResult(result, "bool", CheckBindingAsync);
        await TryAddResult(result, "double", CheckBindingAsync);
        await TryAddResult(result, "float", CheckBindingAsync);
        await TryAddResult(result, "byte", CheckBindingAsync);
        await TryAddResult(result, "byte[]", CheckBindingAsync);
        await TryAddResult(result, "sbyte", CheckBindingAsync);
        await TryAddResult(result, "char", CheckBindingAsync);
        await TryAddResult(result, "decimal", CheckBindingAsync);
        await TryAddResult(result, "uint", CheckBindingAsync);
        await TryAddResult(result, "long", CheckBindingAsync);
        await TryAddResult(result, "ulong", CheckBindingAsync);
        await TryAddResult(result, "short", CheckBindingAsync);
        await TryAddResult(result, "ushort", CheckBindingAsync);

        return result;
    }

    private static async Task TryAddResult(JsonObject jsonObject, string key, Func<string, Task<JsonNode?>> action)
    {
        var result = await action(key);
        if (result != null)
        {
            jsonObject.Add(key, result);
        }
    }
}
