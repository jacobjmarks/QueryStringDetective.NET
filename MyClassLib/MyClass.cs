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
                endpoints.MapGet("/string", ([FromQuery] string q) => JsonSerializer.Serialize(q));
                endpoints.MapGet("/string-array", ([FromQuery] string[] q) => JsonSerializer.Serialize(q));
            });
        });

        return new TestServer(builder);
    }

    public static async Task<object> TestQueryStringBindingAsync(string queryString)
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
        await TryAddResult(result, "string-array", CheckBindingAsync);

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
