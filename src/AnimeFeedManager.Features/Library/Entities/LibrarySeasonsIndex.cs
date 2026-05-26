using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Single-document catalog of seasons that have been imported into the library.
/// Lives in the <c>system</c> Cosmos container, sharing the
/// <see cref="SystemDocument.SystemPartitionKey"/> partition with all other
/// system documents, and is read/upserted as one record on every import.
/// </summary>
[CosmosEntity(CosmosContainers.System, "/partitionKey")]
public sealed record LibrarySeasonsIndex : SystemDocument
{
    public const string DocumentId = "library-seasons-index";

    public ImmutableArray<SeasonEntry> Seasons { get; init; } = ImmutableArray<SeasonEntry>.Empty;
}


