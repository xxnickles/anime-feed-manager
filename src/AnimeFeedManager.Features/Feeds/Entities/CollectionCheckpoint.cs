namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// Watermark for a collection source — one document per <see cref="CollectionSource"/>,
/// partitioned by the source name so it shares a partition with that source's
/// <see cref="CollectionRun"/> history. Lets a run process only what's new since last check.
/// </summary>
public sealed record CollectionCheckpoint : FeedsDocument
{
    public CollectionSource Source { get; }

    public string? LastSeenGuid { get; init; }
    public DateTimeOffset? LastSeenPublishedAt { get; init; }

    public CollectionCheckpoint(CollectionSource source)
    {
        Source = source;
        Id = $"checkpoint:{source}";
        PartitionKey = source.ToString();
    }
}
