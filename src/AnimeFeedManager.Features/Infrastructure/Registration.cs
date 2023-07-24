using AnimeFeedManager.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Features.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection RegisterStorage(this IServiceCollection services, string connectionString)
    {
        
        services.Configure<AzureBlobStorageOptions>(options =>
        {
            options.StorageConnectionString = connectionString;
        });

        var tableClient = new TableServiceClient(connectionString);
        services.TryAddSingleton<IDomainPostman, AzureQueueMessages>();
        services.TryAddSingleton(typeof(ITableClientFactory<>), typeof(TableClientFactory<>));
        services.TryAddSingleton(tableClient);

        return services;
    }
}