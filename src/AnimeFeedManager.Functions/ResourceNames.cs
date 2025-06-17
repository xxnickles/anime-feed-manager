namespace AnimeFeedManager.Functions;

internal struct Constants
{
    internal const string AzureConnectionName = "AzureWebJobsStorage";
    internal const string SignalRConnectionName = "SignalRConnectionString";

}

internal struct HubNames
{
    public const string Notifications = "notifications";
}


internal struct ServerNotifications
{
    public const string AlertNotifications = "alertnotifications";
}
