using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Features.Infrastructure;

public static class Registration
{
    public static void RegisterStorageBasedServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDomainPostman, AzureQueuePostman>();
        services.TryAddSingleton<ITableClientFactory, TableClientFactory>();
    }

    public static void RegisterResourceCreator(this IServiceCollection services)
    {
        services.TryAddSingleton<ResourceCreator>();
    } 
}