using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Server.Functions;

public class MyFunction : IDisposable
{
    [Function("MyFunction")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
        string qs)
    {
        if (string.IsNullOrWhiteSpace(qs) || !qs.StartsWith("?q="))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var bindingResults = await Server.Core.MyClass.TestQueryStringBindingAsync(qs);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(bindingResults);
        return response;
    }

    public void Dispose()
    {
        Server.Core.MyClass.Dispose();
        GC.SuppressFinalize(this);
    }
}
