namespace AnimeFeedManager.Features.Tv.Library.Storage;

public delegate Task<Result<Unit>> TvLibraryStorageUpdater(IEnumerable<AnimeInfoStorage> series,
    CancellationToken cancellationToken = default);

public static class TvLibraryStore
{
    public static TvLibraryStorageUpdater GetTvLibraryUpdater(this ITableClientFactory clientFactory) => (series, token) =>
        clientFactory.GetClient<AnimeInfoStorage>()
            .Bind(client => client.UpsertSeries(series, token));
    
    private static Task<Result<Unit>> UpsertSeries(
        this AppTableClient tableClient,
        IEnumerable<AnimeInfoStorage> series,
        CancellationToken cancellationToken = default)
    {
        return tableClient.AddBatch(series, cancellationToken);
        
    }
}