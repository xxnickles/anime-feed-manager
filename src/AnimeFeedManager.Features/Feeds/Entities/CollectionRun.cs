namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// One record per collection job execution — the observability trail (graphs are built from
/// this). Partitioned by source name, alongside that source's <see cref="CollectionCheckpoint"/>.
/// Implements <see cref="IPersistedEvent"/> via computed properties over the fields below (no
/// stored-shape change) so it can plug into the admin activity feed alongside <c>LibraryEvent</c>.
/// </summary>
public sealed record CollectionRun : FeedsDocument, IPersistedEvent
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

    public DateTimeOffset OccurredAt => CompletedAt ?? StartedAt;
    public string Kind => "run";
    public Outcome Outcome => Errors.Length > 0 ? Outcome.Error : UnmatchedCount > 0 ? Outcome.Warning : Outcome.Success;

    public string Summary => Errors.Length > 0
        ? string.Join("; ", Errors)
        : $"{ItemsScanned} scanned, {MatchedCount} matched, {UnmatchedCount} unmatched";

    // Explicit: CollectionRun's own Source is the typed CollectionSource enum, used everywhere
    // else in Feeds; this only surfaces the string form when held through IPersistedEvent.
    string IPersistedEvent.Source => Source.ToString();
}
