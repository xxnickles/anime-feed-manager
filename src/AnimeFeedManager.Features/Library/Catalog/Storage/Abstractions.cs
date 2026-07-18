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
/// Cross-partition, filtered, projected query: every <c>CurrentlyAiring</c> series outside
/// <paramref name="excludedSeason"/> — the long-running/daily shows (e.g. Detective Conan)
/// that keep airing well past the season they were imported into. Fan-out is bounded by
/// season count (slow-growing), and the status filter keeps the match set small regardless
/// of library size, so this is safe to run on the same cadence as the current-season load.
/// </summary>
public delegate Task<Result<ImmutableArray<SeriesTitleProjection>>> CurrentlyAiringSeriesTitlesOutsideSeasonLoader(
    SeriesSeason excludedSeason,
    CancellationToken cancellationToken);

/// <summary>
/// Cross-partition, projected query: every series outside <paramref name="excludedSeason"/>,
/// regardless of status — the reconciliation (cold path) candidate set. Broader than
/// <see cref="CurrentlyAiringSeriesTitlesOutsideSeasonLoader"/> (no status filter), since late
/// Nyaa releases (batch/BD-remux) can land against a series from any prior season. Titles-only
/// projection keeps this cheap regardless of library age.
/// </summary>
public delegate Task<Result<ImmutableArray<SeriesTitleProjection>>> SeriesTitlesOutsideSeasonLoader(
    SeriesSeason excludedSeason,
    CancellationToken cancellationToken);
