namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// A significant Feeds occurrence (currently: Jikan degraded-during-classification) persisted for
/// later insights — Feeds' counterpart to Library's <c>LibraryEvent</c>. Partitioned by
/// <see cref="Source"/> rather than the per-series/per-job-source partitions the rest of this
/// container uses, same reasoning as <c>LibraryEvent</c>: a growing event stream doesn't belong
/// crammed into one of those.
/// </summary>
public sealed record FeedsOccurrence : FeedsDocument, IPersistedEvent
{
    public string Source { get; }
    public string Kind { get; init; } = string.Empty;
    public Outcome Outcome { get; init; }
    public string Summary { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }

    public FeedsOccurrence(string source)
    {
        Source = source;
        PartitionKey = source;
    }
}
