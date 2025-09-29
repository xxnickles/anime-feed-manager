namespace AnimeFeedManager.Features.Tv.Library.Storage;

public delegate Task<Result<Unit>> TvLibraryStorageUpdater(IEnumerable<AnimeInfoStorage> series,
    CancellationToken cancellationToken = default);

public delegate Task<Result<Unit>> TvSeriesStorageUpdater(AnimeInfoStorage series,
    CancellationToken cancellationToken = default);

public static class TvLibraryStore
{
    public static TvLibraryStorageUpdater GetTvLibraryUpdater(this ITableClientFactory clientFactory) =>
        (series, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.UpsertSeries(series, token));

    public static TvSeriesStorageUpdater GetTvSeriesUpdater(this ITableClientFactory clientFactory) =>
        (series, token) =>
            clientFactory.GetClient<AnimeInfoStorage>().Bind(client => client.UpdateSeries(series, token));

    private static Task<Result<Unit>> UpsertSeries(
        this TableClient tableClient,
        IEnumerable<AnimeInfoStorage> series,
        CancellationToken cancellationToken = default)
    {
        return tableClient.AddBatch(series, cancellationToken)
            .MapError(error => error
                .WithLogProperty("Count", series.Count())
                .WithOperationName(nameof(UpsertSeries)));
    }

    private static Task<Result<Unit>> UpdateSeries(
        this TableClient tableClient,
        AnimeInfoStorage series,
        CancellationToken token) =>
        tableClient.TryExecute<AnimeInfoStorage>(client =>
                client.UpdateEntityAsync(series, series.ETag, cancellationToken: token))
            .WithDefaultMap()
            .MapError(error => error
                .WithLogProperty("Series", series)
                .WithOperationName(nameof(UpdateSeries)));
}