namespace AnimeFeedManager.Features.Tv.Library.Storage;

public delegate Task<Result<Unit>> TvLibraryUpdater(IEnumerable<AnimeInfoStorage> series,
    CancellationToken cancellationToken = default);

public static class TvLibraryStore
{
    public static TvLibraryUpdater UpsertLibrary(ITableClientFactory clientFactory) => (series, token) =>
        clientFactory.GetClient<AnimeInfoStorage>(token)
            .Bind(client => client.UpsertSeries(series, token));
    
    private static Task<Result<Unit>> UpsertSeries(
        this AppTableClient<AnimeInfoStorage> tableClient,
        IEnumerable<AnimeInfoStorage> series,
        CancellationToken cancellationToken = default)
    {
        return tableClient.AddBatch(series, cancellationToken);
        
    }
}