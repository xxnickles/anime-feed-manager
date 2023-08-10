using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.IO;

public sealed class MoviesStorage : IMoviesStorage
{
    private readonly ITableClientFactory<MovieStorage> _tableClientFactory;

    public MoviesStorage(ITableClientFactory<MovieStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }
    
    public Task<Either<DomainError, Unit>> Add(ImmutableList<MovieStorage> series, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series,  token))
            .MapAsync(_ => unit);
    }
}