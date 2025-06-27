namespace AnimeFeedManager.Features.Tv.Library.Storage;

public delegate Task<Result<Unit>> FeedTitlesUpdater(FeedTitlesStorage titles,
    CancellationToken cancellationToken = default);


public static class FeedTitlesStore
{
    public static FeedTitlesUpdater GetFeedTitlesUpdater(this ITableClientFactory clientFactory) => (titles, token) =>
        clientFactory.GetClient<FeedTitlesStorage>(token)
            .Bind(client => client.UpsertFeedTitles(titles, token));
    
    private static Task<Result<Unit>> UpsertFeedTitles(
        this AppTableClient<FeedTitlesStorage> tableClient,
        FeedTitlesStorage titles,
        CancellationToken cancellationToken = default)
    {
        return tableClient.TryExecute(client => client.UpsertEntityAsync(titles, cancellationToken: cancellationToken))
            .WithDefaultMap();
        
    }
}