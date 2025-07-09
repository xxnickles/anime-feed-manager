namespace AnimeFeedManager.Features.User.TvSubscriptions.Storage;

[WithTableName(AzureTableMap.StoreTo.ProcessedSubscriptions)]
public sealed class ProcessedSubscriptionsStorage : ITableEntity
{
    public string? PartitionKey { get; set; }
    public string? RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}