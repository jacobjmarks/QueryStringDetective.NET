using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Server.Core;

namespace Server.Functions;

public class EvaluateQueryBinding(QueryBindingEvaluator bindingEvaluator)
{
    private readonly QueryBindingEvaluator _bindingEvaluator = bindingEvaluator;

    [Function(nameof(EvaluateQueryBinding))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
        string qs)
    {
        if (string.IsNullOrWhiteSpace(qs) || !qs.StartsWith('?'))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var bindingResults = await _bindingEvaluator.EvaluateAsync(qs);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(bindingResults);
        return response;
    }
}
