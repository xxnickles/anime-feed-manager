using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Infrastructure.TableStorage;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection RegisterStorage(this IServiceCollection services, string connectionString)
    {
        
        services.Configure<AzureBlobStorageOptions>(options =>
        {
            options.StorageConnectionString = connectionString;
        });

        // services.TryAddSingleton<IImagesStore, AzureStorageBlobStore>();
        
        var tableClient = new TableServiceClient(connectionString);
        services.AddSingleton<IQueueResolver, QueueResolver>();
        services.AddSingleton<IDomainPostman, AzureQueueMessages>();
        services.AddSingleton(typeof(ITableClientFactory<>), typeof(TableClientFactory<>));
        services.AddSingleton(tableClient);

        return services;
    }
}