namespace AnimeFeedManager.Storage.Domain;

public class NotificationStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string? Payload { get; set; }
    public string? For { get; set; }
    public string? Type { get; set; }
}
