using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Library.Import.Storage;

public delegate Task<Result<BulkResult<Unit>>> SeriesPersistenceHandler(Series[] series, CancellationToken cancellationToken);

public delegate Task<Result<LibrarySeasonsIndex>> LibrarySeasonsIndexLoader(CancellationToken cancellationToken);

public delegate Task<Result<Unit>> LibrarySeasonsIndexUpserter(LibrarySeasonsIndex index, CancellationToken cancellationToken);