namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// One record per collection job execution — the observability trail (graphs are built from
/// this). Partitioned by source name, alongside that source's <see cref="CollectionCheckpoint"/>.
/// </summary>
public sealed record CollectionRun : FeedsDocument
{
    public CollectionSource Source { get; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }

    public int ItemsScanned { get; init; }
    public int NewSinceCheckpoint { get; init; }
    public int MatchedCount { get; init; }
    public int UnmatchedCount { get; init; }
    public string[] Errors { get; init; } = [];

    public CollectionRun(CollectionSource source)
    {
        Source = source;
        PartitionKey = source.ToString();
    }
}
