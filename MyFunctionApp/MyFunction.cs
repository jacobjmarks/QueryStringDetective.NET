using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyFunctionApp;

public class MyFunction
{
    [Function("MyFunction")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
        [FromQuery(Name = "q")] string queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString) || !queryString.StartsWith("?q="))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var bindingResults = await MyClassLib.MyClass.TestQueryStringBindingAsync(queryString);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(bindingResults);
        return response;
    }
}
