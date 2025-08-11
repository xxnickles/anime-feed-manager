using Microsoft.Extensions.Hosting;

namespace AnimeFeedManager.Services.Shared;

public static class StorageRegistrationConstants
{
    public const string BlobConnection = "BlobConnection";
    public const string QueueConnection = "QueueConnection";
    public const string TablesConnection = "TablesConnection";
    public const string SignalRConnectionName = "SignalRConnectionString";
}


public static class HubNames
{
    public const string Notifications = "notifications";
}


public static class ServerNotifications
{
    public const string AlertNotifications = "alertnotifications";
}


public static class StorageRegistration
{
    public static void RegisterStorageServices(this IHostApplicationBuilder builder) {
        builder.AddAzureBlobServiceClient(StorageRegistrationConstants.BlobConnection);
        builder.AddAzureQueueServiceClient(StorageRegistrationConstants.QueueConnection);
        builder.AddAzureTableServiceClient(StorageRegistrationConstants.TablesConnection);
        
    }
}