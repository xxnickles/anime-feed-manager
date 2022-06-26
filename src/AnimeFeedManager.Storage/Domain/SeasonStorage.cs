namespace AnimeFeedManager.Storage.Domain;

public class SeasonStorage: ITableEntity
{
    public string? Season { get; set; }
    public int Year { get; set; } // Azure tables only works with Int and Int64
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}