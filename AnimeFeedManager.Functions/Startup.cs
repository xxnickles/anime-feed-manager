using AnimeFeedManager.DI;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AnimeFeedManager.Functions.Startup))]
namespace AnimeFeedManager.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .RegisterStorage("UseDevelopmentStorage=true")
                .RegisterAppServices()
                .RegisterApplicationServices();
        }
    }
}
