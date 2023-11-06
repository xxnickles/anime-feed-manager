using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.IO;

public interface IMoviesStorage
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<MovieStorage> series, CancellationToken token);
}

public sealed class MoviesStorage(ITableClientFactory<MovieStorage> tableClientFactory) : IMoviesStorage
{
    public Task<Either<DomainError, Unit>> Add(ImmutableList<MovieStorage> series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series, token))
            .MapAsync(_ => unit);
    }
}