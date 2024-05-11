using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Server.Functions;

public class CheckHealth(HealthCheckService healthCheckService)
{
    private readonly HealthCheckService _healthCheckService = healthCheckService;

    [Function(nameof(CheckHealth))]
    public async Task<HttpResponseData> CheckHealthAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/health")] HttpRequestData req)
    {
        var healthReport = await _healthCheckService.CheckHealthAsync();
        return req.CreateResponse(healthReport.Status switch
        {
            HealthStatus.Unhealthy => HttpStatusCode.ServiceUnavailable,
            _ => HttpStatusCode.OK,
        });
    }
}
