using AnimeFeedManager.Features.Common.Storage;

namespace AnimeFeedManager.Features.Tv.Library.Storage;

[WithTableName(AzureTableMap.StoreTo.JsonStorage)]
public class FeedTitlesStorage : JsonStorage
{
    public const string Partition = "feed-titles";
    public const string Key = "standard";

    public override string PartitionKey { get; set; } = Partition;
    public override string RowKey { get; set; } = Key;
}