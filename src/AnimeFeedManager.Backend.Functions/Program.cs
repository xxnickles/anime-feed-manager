using AnimeFeedManager.Backend.Functions.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.RegisterAppDependencies();
    })
    .Build();

host.Run();
