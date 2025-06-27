namespace AnimeFeedManager.Features.Common.Storage;

public abstract class JsonStorage : ITableEntity
{
    public abstract string PartitionKey { get; set; }
    public string? Payload { get; set; }
    public abstract string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}