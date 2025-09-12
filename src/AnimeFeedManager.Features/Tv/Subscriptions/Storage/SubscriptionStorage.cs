namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage;

public enum SubscriptionType
{
    None,
    Interested,
    Subscribed
}

public enum SubscriptionStatus
{
    None,
    Active,
    Expired,
}

[WithTableName(AzureTableMap.StoreTo.Subscriptions)]
public sealed class SubscriptionStorage : ITableEntity
{
    public string? PartitionKey { get; set; } // represents the user
    public string? RowKey { get; set; } // represents the series id
    public SubscriptionType Type { get; set; } = SubscriptionType.None;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.None;
    public string SeriesTitle { get; set; } = string.Empty; 
    public string? SeriesFeedTitle { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}