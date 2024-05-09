using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Core;

var builder = new HostBuilder();
builder.ConfigureFunctionsWorkerDefaults();
builder.ConfigureServices(services =>
{
    services.AddSingleton<QueryBindingEvaluator>();
    services.AddHealthChecks();
});

using var host = builder.Build();
await host.RunAsync();
