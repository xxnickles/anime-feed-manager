namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public class OvaFeedStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string? Payload { get; set; }
}