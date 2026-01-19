using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Features.Infrastructure;

public static class Registration
{
    extension(IServiceCollection services)
    {
        public void RegisterStorageBasedServices()
        {
            services.TryAddSingleton<IDomainPostman, AzureQueuePostman>();
            services.TryAddSingleton<ITableClientFactory, TableClientFactory>();
        }

        public void RegisterResourceCreator()
        {
            services.TryAddSingleton<ResourceCreator>();
        }
    }
}