using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Storage.Domain;

public class NotificationStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string? Payload { get; set; }
    public NotificationType Type { get; set; }
}
