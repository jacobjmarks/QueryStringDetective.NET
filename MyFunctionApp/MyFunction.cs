using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyFunctionApp;

public class MyFunction : IDisposable
{
    [Function("MyFunction")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
        string q)
    {
        if (string.IsNullOrWhiteSpace(q) || !q.StartsWith("?q="))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var bindingResults = await MyClassLib.MyClass.TestQueryStringBindingAsync(q);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(bindingResults);
        return response;
    }

    public void Dispose()
    {
        MyClassLib.MyClass.Dispose();
        GC.SuppressFinalize(this);
    }
}
