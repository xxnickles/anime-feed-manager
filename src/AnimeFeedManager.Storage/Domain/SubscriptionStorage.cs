namespace AnimeFeedManager.Storage.Domain;

public class SubscriptionStorage : ITableEntity
{
    public string?PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}