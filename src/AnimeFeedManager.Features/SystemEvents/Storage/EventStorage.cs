namespace AnimeFeedManager.Features.SystemEvents.Storage;

[WithTableName(AzureTableMap.StoreTo.Events)]
public class EventStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public EventTarget Target { get; set; } = EventTarget.LocalStorage;
    public EventType EventType { get; set; } = EventType.Information;
    public string PayloadTypeName { get; set; } = string.Empty;
    public string PayloadData { get; set; } = string.Empty;
}