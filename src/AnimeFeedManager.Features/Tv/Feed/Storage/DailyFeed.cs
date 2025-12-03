using AnimeFeedManager.Features.Common.Storage;

namespace AnimeFeedManager.Features.Tv.Feed.Storage;

[WithTableName(AzureTableMap.StoreTo.JsonStorage)]
public class DailyFeedStorage : JsonStorage
{
    public const string Partition = "daily-feed";
    public const string Key = "daily-feed";
    public override string PartitionKey { get; set; } = Partition;
    public override string RowKey { get; set; } = Key; 
}