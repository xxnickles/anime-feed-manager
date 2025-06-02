namespace AnimeFeedManager.Features.Seasons.Storage;

public sealed class SeasonStorage: ITableEntity
{
    public string? Season { get; set; }
    public int Year { get; set; } // Azure tables only works with Int and Int64
    public string? PartitionKey { get; set; }
    
    public bool Latest { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public sealed class LatestSeasonsStorage: ITableEntity
{
    public const string Partition = "latest-seasons";
    public const string Key = "last-4";
    
    public string PartitionKey { get; set; } = Partition;
    public string? Payload { get; set; }
    public string RowKey { get; set; } = Key;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    
}