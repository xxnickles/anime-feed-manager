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

/// <summary>
/// Cross-partition, projected query: titles for each of <paramref name="malIds"/>, regardless of
/// season — the notification-dispatch job resolves titles for whatever series had a live release
/// this pass, with no season on hand to scope a partition read. Same projection as
/// <see cref="SeriesTitleProjection"/> (its <c>AllTitles[0]</c> is always the canonical default
/// title — see <c>JikanSeriesMapper.BuildAllTitles</c>). Ids with no matching series are silently
/// omitted, not errors.
/// </summary>
public delegate Task<Result<ImmutableArray<SeriesTitleProjection>>> SeriesTitlesByIdsLoader(
    ImmutableArray<int> malIds,
    CancellationToken cancellationToken);
