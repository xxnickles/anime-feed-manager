using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;
using OvaContainer =
    (AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage.OvaStorage Ova, Azure.Data.Tables.TableClient Client);
using OvasContainer =
    (System.Collections.Immutable.ImmutableList<
        AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage.OvaStorage> Ovas, Azure.Data.Tables.TableClient
    Client);

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.IO;

public interface IOvaFeedRemover
{
    Task<Either<DomainError, Unit>> RemoveFeed(RowKey rowKey, PartitionKey key, CancellationToken token);
}

public sealed class OvaFeedRemover : IOvaFeedRemover
{
    private readonly ITableClientFactory<OvaStorage> _tableClientFactory;

    public OvaFeedRemover(ITableClientFactory<OvaStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> RemoveFeed(RowKey rowKey, PartitionKey key, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithNotFoundResult(() =>
                    client.QueryAsync<OvaStorage>(storage => storage.PartitionKey == key && storage.RowKey == rowKey,
                        cancellationToken: token))
                .MapAsync(items => (items, client)))
            .MapAsync(ReMap)
            .BindAsync(container => TableUtils.TryExecute(() =>
                container.Client.UpsertEntityAsync(container.Ova, TableUpdateMode.Merge, token))).MapAsync(_ => unit);
    }


    private static OvaContainer ReMap(OvasContainer container)
    {
        // Guarantee we have at lest 1 item
        var item = container.Ovas.First();
        item.Status = ShortSeriesStatus.NotProcessed;
        item.FeedInfo = string.Empty;
        return (item, container.Client);
    }
}