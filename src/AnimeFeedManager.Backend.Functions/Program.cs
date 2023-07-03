using AnimeFeedManager.Backend.Functions.Bootstrapping;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.RegisterAppDependencies();
    })
    .Build();

host.Run();
