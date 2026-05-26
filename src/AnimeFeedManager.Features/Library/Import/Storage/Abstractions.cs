using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Library.Import.Storage;

public delegate Task<Result<T>> SingleSeriesPersistenceHandler<T>(Series series, CancellationToken cancellationToken);
