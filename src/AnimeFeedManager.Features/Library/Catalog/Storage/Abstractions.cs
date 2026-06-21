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
