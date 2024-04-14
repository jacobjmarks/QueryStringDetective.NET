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

        var result = new JsonObject();

        var bindingChecks = new string[]
            { "string", "string-array", }
            .Select(endpoint => Task.Run(async () =>
            {
                using var response = await minimalApiClient.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                    return;

                var responseContent = await response.Content.ReadAsStringAsync();
                var bindingResult = JsonNode.Parse(responseContent);
                result.Add(endpoint, bindingResult);
            }));

        await Task.WhenAll(bindingChecks);

        return result;
    }
}
