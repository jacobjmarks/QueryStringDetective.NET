using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Server.Functions;

public class EvaluateQueryBinding : IDisposable
{
    [Function("EvaluateQueryBinding")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
        string qs)
    {
        if (string.IsNullOrWhiteSpace(qs) || !qs.StartsWith("?q="))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var bindingResults = await Server.Core.QueryBindingEvaluator.EvaluateAsync(qs);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(bindingResults);
        return response;
    }

    public void Dispose()
    {
        Server.Core.QueryBindingEvaluator.Dispose();
        GC.SuppressFinalize(this);
    }
}
