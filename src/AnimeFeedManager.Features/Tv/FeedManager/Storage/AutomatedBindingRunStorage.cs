using AnimeFeedManager.Features.Common.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.Storage;

/// <summary>The latest automated binding run; <see cref="JsonStorage.Payload"/> is an <c>AutomatedBindingCompleted</c>.</summary>
[WithTableName(AzureTableMap.StoreTo.JsonStorage)]
public sealed class AutomatedBindingRunStorage : JsonStorage
{
    public const string Partition = "automated-binding";
    public const string Key = "latest";

    public override string PartitionKey { get; set; } = Partition;
    public override string RowKey { get; set; } = Key;
}
