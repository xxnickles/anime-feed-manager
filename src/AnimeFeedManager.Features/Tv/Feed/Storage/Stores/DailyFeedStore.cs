using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Feed.Storage.Stores;

public delegate Task<Result<Unit>> DailyFeedUpdater(DailySeriesFeed[] dailySeriesFeeds,
    CancellationToken cancellationToken = default);

public static class DailyFeedStore
{
    extension(ITableClientFactory clientFactory)
    {
        public DailyFeedUpdater TableStorageFeedUpdater => (dailySeriesFeeds,
                cancellationToken) =>
            clientFactory.GetClient<DailyFeedStorage>()
                .Bind(client => client.UpsertDailyFeeds(dailySeriesFeeds, cancellationToken));
    }


    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> UpsertDailyFeeds(DailySeriesFeed[] dailySeriesFeeds,
            CancellationToken cancellationToken = default)
        {
            var entity = new DailyFeedStorage
            {
                Payload = JsonSerializer.Serialize(dailySeriesFeeds, DailyFeedContext.Default.DailySeriesFeedArray)
            };

            return tableClient.TryExecute<DailyFeedStorage>(client =>
                    client.UpsertEntityAsync(entity, cancellationToken: cancellationToken))
                .WithDefaultMap()
                .MapError(error =>
                    error.WithLogProperty("Feed", dailySeriesFeeds).WithOperationName(nameof(UpsertDailyFeeds)));
        }
    }
}