using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;

namespace AnimeFeedManager.Features.Library.Seasons;

/// <summary>
/// Reads the single <see cref="LibrarySeasonsIndex"/> document. Returns an
/// empty index on first call (Cosmos 404 → success with default).
/// </summary>
public delegate Task<Result<LibrarySeasonsIndex>> LibrarySeasonsIndexLoader(CancellationToken cancellationToken);

/// <summary>
/// Registers a season in the index. The implementation owns the point-read +
/// merge + upsert cycle, so callers only need to supply the entry to add or
/// replace. Replacement is by <see cref="SeriesSeason"/> equality.
/// </summary>
public delegate Task<Result<Unit>> LibrarySeasonsIndexUpserter(SeasonEntry entry, CancellationToken cancellationToken);
