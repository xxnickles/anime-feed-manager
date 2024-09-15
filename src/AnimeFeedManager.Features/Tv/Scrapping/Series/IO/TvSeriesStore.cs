using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface ITvSeriesStore
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series, CancellationToken token);

    Task<Either<DomainError, Unit>> RemoveSeries(RowKey rowKey, PartitionKey key, CancellationToken token);
}

public sealed class TvSeriesStore(ITableClientFactory<AnimeInfoStorage> tableClientFactory) : ITvSeriesStore
{
    public Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series, token))
            .MapAsync(_ => unit);
    }
    
    public Task<Either<DomainError, Unit>> RemoveSeries(RowKey rowKey, PartitionKey key, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.DeleteEntityAsync(key, rowKey, cancellationToken: token)))
            .MapAsync(_ => unit);
    }
}