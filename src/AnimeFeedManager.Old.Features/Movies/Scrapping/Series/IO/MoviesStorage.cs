using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Movies.Scrapping.Series.IO;

public interface IMoviesStorage
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<MovieStorage> series, CancellationToken token);
    
    Task<Either<DomainError, Unit>> RemoveMovie(RowKey rowKey, PartitionKey key, CancellationToken token);
    
    Task<Either<DomainError, Unit>> Update(MovieStorage series, CancellationToken token);
}

public sealed class MoviesStorage(ITableClientFactory<MovieStorage> tableClientFactory) : IMoviesStorage
{
    public Task<Either<DomainError, Unit>> Add(ImmutableList<MovieStorage> series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series, token))
            .MapAsync(_ => unit);
    }
    
    public Task<Either<DomainError, Unit>> RemoveMovie(RowKey rowKey, PartitionKey key, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.DeleteEntityAsync(key, rowKey, cancellationToken: token)))
            .MapAsync(_ => unit);
    }

    public Task<Either<DomainError, Unit>> Update(MovieStorage series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.TryExecute(() => client.UpsertEntityAsync(series, TableUpdateMode.Merge, token)))
            .MapAsync(_ => unit);
    }
}