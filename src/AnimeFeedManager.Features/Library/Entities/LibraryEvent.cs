namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// A significant Library occurrence (currently: import failures) persisted for later insights.
/// Lives in the <c>system</c> Cosmos container alongside <see cref="LibrarySeasonsIndex"/>, but
/// partitions by <see cref="Source"/> rather than the shared <see cref="SystemDocument.SystemPartitionKey"/>
/// singleton value — a growing event stream doesn't belong crammed into that one pinned partition.
/// </summary>
[CosmosEntity(CosmosContainers.System, "/partitionKey")]
public sealed record LibraryEvent : SystemDocument, IPersistedEvent
{
    public string Source { get; }
    public string Kind { get; init; } = string.Empty;
    public Outcome Outcome { get; init; }
    public string Summary { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; }

    public LibraryEvent(string source)
    {
        Source = source;
        PartitionKey = source;
    }
}
