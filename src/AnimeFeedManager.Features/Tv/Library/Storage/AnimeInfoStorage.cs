namespace AnimeFeedManager.Features.Tv.Library.Storage;


[WithTableName(AzureTableMap.StoreTo.AnimeLibrary)]
public sealed class UpdateFeedAnimeInfoStorage : ITableEntity 
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string? FeedTitle { get; set; }
    public string? Status { get; set; }
}

[WithTableName(AzureTableMap.StoreTo.AnimeLibrary)]
public sealed class AnimeInfoStorage : ITableEntity
{
    public string? Title { get; set; }
    public string? Synopsis { get; set; }
    public string? FeedTitle { get; set; }
    public DateTime? Date { get; set; }
    public string Status { get; set; } = SeriesStatus.NotAvailable;
    
    public string? AlternativeTitles { get; set; }
    public string? PartitionKey { get; set; }
    public string? ImagePath { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
