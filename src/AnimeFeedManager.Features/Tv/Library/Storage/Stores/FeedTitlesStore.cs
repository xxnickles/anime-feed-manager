namespace AnimeFeedManager.Features.Tv.Library.Storage.Stores;

public delegate Task<Result<Unit>> FeedTitlesUpdater(FeedTitlesStorage titles,
    CancellationToken cancellationToken = default);

public static class FeedTitlesStore
{
    extension(ITableClientFactory clientFactory)
    {
        public FeedTitlesUpdater TableStorageFeedTitlesUpdater => (titles, token) =>
            clientFactory.GetClient<FeedTitlesStorage>()
                .WithOperationName(nameof(FeedTitlesStore))
                .WithLogProperty("Titles", titles)
                .Bind(client => client.UpsertFeedTitles(titles, token));
    }


    private static Task<Result<Unit>> UpsertFeedTitles(
        this TableClient tableClient,
        FeedTitlesStorage titles,
        CancellationToken cancellationToken = default)
    {
        return tableClient.TryExecute<FeedTitlesStorage>(client =>
                client.UpsertEntityAsync(titles, cancellationToken: cancellationToken))
            .WithDefaultMap();
    }
}