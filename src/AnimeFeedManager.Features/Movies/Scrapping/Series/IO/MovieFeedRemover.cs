using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;
using MoviesContainer =
    (System.Collections.Immutable.ImmutableList<
        AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage.MovieStorage> Movies,
    Azure.Data.Tables.TableClient
    Client);
using MovieContainer =
    (AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage.MovieStorage Movie, Azure.Data.Tables.TableClient
    Client);

namespace AnimeFeedManager.Features.Movies.Scrapping.Series.IO;

public interface IMovieFeedRemover
{
    Task<Either<DomainError, Unit>> RemoveFeed(RowKey rowKey, PartitionKey key, CancellationToken token);
}

public class MovieFeedRemover : IMovieFeedRemover
{
    private readonly ITableClientFactory<MovieStorage> _tableClientFactory;

    public MovieFeedRemover(ITableClientFactory<MovieStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> RemoveFeed(RowKey rowKey, PartitionKey key, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithNotFoundResult(() =>
                    client.QueryAsync<MovieStorage>(storage => storage.PartitionKey == key && storage.RowKey == rowKey,
                        cancellationToken: token))
                .MapAsync(items => (items, client)))
            .MapAsync(ReMap)
            .BindAsync(container => TableUtils.TryExecute(() =>
                container.Client.UpsertEntityAsync(container.Movie, TableUpdateMode.Merge, token))).MapAsync(_ => unit);
    }

    private static MovieContainer ReMap(MoviesContainer container)
    {
        // Guarantee we have at lest 1 item
        var item = container.Movies.First();
        item.Status = ShortSeriesStatus.NotProcessed;
        item.FeedInfo = string.Empty;
        return (item, container.Client);
    }
}