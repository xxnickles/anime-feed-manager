using System.Diagnostics.CodeAnalysis;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

namespace AnimeFeedManager.Features.Infrastructure;

public static class InfrastructureRegistration
{
    private const string TableBaseUrl = "https://{0}.table.core.windows.net";
    private const string QueueBaseUrl = "https://{0}.queue.core.windows.net";
    private const string BlobBaseUrl = "https://{0}.blob.core.windows.net";


    public static IServiceCollection RegisterStorage(this IServiceCollection services,
        string connectionString)
    {
        RegisterWithConnectionString(services, connectionString);
        services.RegisterCommonServices();
        return services;
    }

    public static IServiceCollection RegisterStorage(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        var storageAccountName = configuration["StorageAccountName"];
        if (!string.IsNullOrEmpty(storageAccountName))
        {
            RegisterWithAzureIdentity(services, storageAccountName);
        }
        else
        {
            RegisterWithConnectionString(services, configuration.GetConnectionString("AzureStorage") ?? string.Empty);
        }

        services.RegisterCommonServices();

        return services;
    }

    private static void RegisterCommonServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDomainPostman, AzureQueueMessages>();
        services.TryAddSingleton(typeof(ITableClientFactory<>), typeof(TableClientFactory<>));
    }

    private static void RegisterWithAzureIdentity(IServiceCollection services, string storageAccountName)
    {
        if (CreateUri(TableBaseUrl, storageAccountName, out var tableUri) &&
            CreateUri(QueueBaseUrl, storageAccountName, out var queueUri) &&
            CreateUri(BlobBaseUrl, storageAccountName, out var blobUri))
        {
            services.TryAddSingleton<AzureStorageSettings>(
                new TokenCredentialSettings(new QueueUri(queueUri), new BlobUri(blobUri)));
            services.TryAddSingleton(new TableServiceClient(tableUri, new DefaultAzureCredential()));
        }
        else
        {
            throw new ArgumentException("Azure Storage resources are malformed.");
        }
    }

    private static void RegisterWithConnectionString(IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton<AzureStorageSettings>(new ConnectionStringSettings(connectionString));
        services.TryAddSingleton(new TableServiceClient(connectionString));
    }

    private static bool CreateUri(string baseUrl, string storageAccountName, [NotNullWhen(true)] out Uri? tableUri)
    {
        return Uri.TryCreate($"{baseUrl}{storageAccountName}", UriKind.Absolute, out tableUri);
    }
}