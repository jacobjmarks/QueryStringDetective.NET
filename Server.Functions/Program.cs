using Microsoft.Extensions.Hosting;

var builder = new HostBuilder();
builder.ConfigureFunctionsWorkerDefaults();

using var host = builder.Build();
await host.RunAsync();
