using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Library.Catalog.Storage;

/// <summary>
/// Reads every <see cref="Series"/> stored under a season's partition (the
/// container is partitioned by <c>/seriesSeason</c>, so this is a single-partition
/// read). Returns an empty array when the season holds no series or was never imported.
/// </summary>
public delegate Task<Result<ImmutableArray<Series>>> SeriesBySeasonLoader(
    SeriesSeason season,
    CancellationToken cancellationToken);

/// <summary>
/// Point-reads a single <see cref="Series"/> by its MAL id within a season's partition
/// (PK = season, document id = malId). A MalId is unique only within a season partition,
/// so the season is required to disambiguate. Returns <c>NotFoundError</c> when that
/// season holds no series with the given id.
/// </summary>
public delegate Task<Result<Series>> SeriesByIdLoader(
    SeriesSeason season,
    int malId,
    CancellationToken cancellationToken);
