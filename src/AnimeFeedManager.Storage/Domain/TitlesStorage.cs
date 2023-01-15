namespace AnimeFeedManager.Storage.Domain;

public sealed class TitlesStorage : ITableEntity
{
    public string? Titles { get; set; }
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}