namespace AnimeFeedManager.Storage.Domain;

public sealed class SubscriptionStorage : ITableEntity
{
    public string?PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}