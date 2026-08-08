namespace AnimeFeedManager.Features.Subscriptions.Entities;

public enum SubscriptionEventKind
{
    Created,
    Removed
}

/// <summary>
/// A subscribe/unsubscribe occurrence, persisted for later insights (e.g. totals-over-time).
/// Lives in the <c>system</c> Cosmos container, partitioned by <see cref="Source"/> rather than the
/// shared <see cref="SystemDocument.SystemPartitionKey"/> singleton value — an unbounded event
/// stream, same reasoning as <c>LibraryEvent</c>/<c>FeedsOccurrence</c>. Implements
/// <see cref="IPersistedEvent"/> via computed properties (no stored-shape change) so it can plug
/// into the admin activity feed alongside those.
/// </summary>
[CosmosEntity(CosmosContainers.System, "/partitionKey")]
public sealed record SubscriptionEvent : SystemDocument, IPersistedEvent
{
    public string Source { get; }
    public string UserId { get; init; } = string.Empty;
    public int SeriesId { get; init; }
    public SubscriptionEventKind Kind { get; init; }
    public DateTimeOffset OccurredAt { get; init; }

    public SubscriptionEvent(string source)
    {
        Source = source;
        PartitionKey = source;
    }

    public Outcome Outcome => Outcome.Info;

    public string Summary => Kind == SubscriptionEventKind.Created
        ? $"User {UserId} subscribed to series {SeriesId}"
        : $"User {UserId} unsubscribed from series {SeriesId}";

    // Explicit: SubscriptionEvent's own Kind is the typed enum, used everywhere else in
    // Subscriptions; this only surfaces the string form when held through IPersistedEvent.
    string IPersistedEvent.Kind => Kind.ToString();
}
