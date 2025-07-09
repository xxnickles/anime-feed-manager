namespace AnimeFeedManager.Features.User.TvSubscriptions.Storage;

[WithTableName(AzureTableMap.StoreTo.InterestedSeries)]
public sealed class InterestedStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public string? SeriesId { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}