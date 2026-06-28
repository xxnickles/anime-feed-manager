using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Library.Seasons;

/// <summary>
/// Reads the single <see cref="LibrarySeasonsIndex"/> document. Returns an
/// empty index on first call (Cosmos 404 → success with default).
/// </summary>
public delegate Task<Result<LibrarySeasonsIndex>> LibrarySeasonsIndexLoader(CancellationToken cancellationToken);

/// <summary>
/// Whether an import targets the currently-airing season — which moves the single
/// <see cref="SeasonEntry.IsCurrent"/> marker — or a specific back-catalog season,
/// which leaves the marker untouched.
/// </summary>
public enum SeasonImportKind
{
    Specific,
    Current
}

/// <summary>
/// Registers a season in the index. The implementation owns the point-read +
/// merge + upsert cycle, so callers supply the entry plus the import
/// <see cref="SeasonImportKind"/>. Replacement is by <see cref="SeriesSeason"/>
/// equality; the kind drives the single-current invariant.
/// </summary>
public delegate Task<Result<Unit>> LibrarySeasonsIndexUpserter(
    SeasonEntry entry, SeasonImportKind kind, CancellationToken cancellationToken);

/// <summary>
/// Resolves the most-recent imported season from the index. Fails with a
/// <see cref="NotFoundError"/> when nothing has been imported yet — absence rides the
/// error channel rather than a nullable success, so callers match on outcome (and can
/// branch on <see cref="NotFoundError"/> for a friendly empty state). Drives the <c>/</c> landing.
/// </summary>
public delegate Task<Result<SeriesSeason>> LatestSeasonResolver(CancellationToken cancellationToken);
