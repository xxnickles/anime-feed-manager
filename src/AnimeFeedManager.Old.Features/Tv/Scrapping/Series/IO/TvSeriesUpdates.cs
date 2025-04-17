using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Types;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.IO;

public interface ITvSeriesUpdates
{
    Task<Either<DomainError, Unit>> Update(RowKey rowKey, PartitionKey partitionKey, string feedTile,
        CancellationToken token);
}

public sealed class TvSeriesUpdates : ITvSeriesUpdates
{
    private readonly ITableClientFactory<UpdateFeedAnimeInfoStorage> _tableClientFactory;

    public TvSeriesUpdates(ITableClientFactory<UpdateFeedAnimeInfoStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }


    public Task<Either<DomainError, Unit>> Update(RowKey rowKey, PartitionKey partitionKey, string feedTile,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() => client.UpsertEntityAsync(
                new UpdateFeedAnimeInfoStorage
                {
                    FeedTitle = feedTile,
                    PartitionKey = partitionKey,
                    RowKey = rowKey,
                    Status = SeriesStatus.Ongoing
                }, cancellationToken: token)))
            .MapAsync(_ => unit);
    }
}