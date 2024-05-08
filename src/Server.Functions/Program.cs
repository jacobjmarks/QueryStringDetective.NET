using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Core;

var builder = new HostBuilder();
builder.ConfigureFunctionsWorkerDefaults();
builder.ConfigureServices(services =>
{
    services.AddSingleton<QueryBindingEvaluator>();
});

using var host = builder.Build();
await host.RunAsync();
