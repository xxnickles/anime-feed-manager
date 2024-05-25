using AnimeFeedManager.Functions.Bootstrapping;
using Azure.Core.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(options =>
    {
        options.Serializer = new JsonObjectSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    })
    .UseDefaultServiceProvider(options => { options.ValidateOnBuild = true; })
    .ConfigureServices(services =>
    {
        services.AddHttpClient();
        services.AddSingleton(TimeProvider.System);
        services.RegisterAppDependencies();
        services.RegisterSendGrid();
    })
    .Build();

host.Run();