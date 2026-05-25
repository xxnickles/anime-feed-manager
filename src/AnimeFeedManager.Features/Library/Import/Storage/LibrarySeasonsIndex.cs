namespace AnimeFeedManager.Features.Library.Import.Storage;

/// <summary>
/// Single-document catalog of seasons that have been imported into the library.
/// Lives in the <c>system</c> Cosmos container, partitioned by <c>id</c>, and
/// is read/upserted as one record on every import.
/// </summary>
[CosmosEntity(CosmosContainers.System, "/id")]
public sealed record LibrarySeasonsIndex : CosmosDocument
{
    public const string DocumentId = "library-seasons-index";

    public ImmutableArray<SeasonEntry> Seasons { get; init; } = ImmutableArray<SeasonEntry>.Empty;
}

/// <summary>
/// One entry per imported season — the typed <see cref="SeriesSeason"/>, when
/// the season was last imported, and how many series the import produced.
/// </summary>
public sealed record SeasonEntry(
    SeriesSeason SeriesSeason,
    DateTimeOffset LastImportedAt,
    int SeriesCount);
