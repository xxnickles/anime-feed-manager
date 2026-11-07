using AnimeFeedManager.Features.Common.Storage;

namespace AnimeFeedManager.Features.Tv.FeedManager.Storage;

/// <param name="At">When the hourly run requested its nyaa poll.</param>
public sealed record NyaaPoll(DateTimeOffset At);

/// <summary>The last hourly poll whose notifications were sent; <see cref="JsonStorage.Payload"/> is a <see cref="NyaaPoll"/>.</summary>
[WithTableName(AzureTableMap.StoreTo.JsonStorage)]
public sealed class NyaaPollStorage : JsonStorage
{
    public const string Partition = "nyaa-poll";
    public const string Key = "latest";

    public override string PartitionKey { get; set; } = Partition;
    public override string RowKey { get; set; } = Key;
}
