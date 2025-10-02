namespace AnimeFeedManager.Features.Tv.Library.Storage;

public delegate Task<Result<Unit>> TvLibraryStorageUpdater(IEnumerable<AnimeInfoStorage> series,
    CancellationToken cancellationToken = default);

public delegate Task<Result<Unit>> TvSeriesStorageUpdater(AnimeInfoStorage series,
    CancellationToken cancellationToken = default);

public delegate Task<Result<Unit>> TvSeriesRemover(string id, string seasonString, CancellationToken token = default);

public static class TvLibraryStore
{
    public static TvLibraryStorageUpdater TableStorageTvLibraryUpdater(this ITableClientFactory clientFactory) =>
        (series, token) =>
            clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.UpsertSeries(series, token));

    public static TvSeriesStorageUpdater TableStorageTvSeriesUpdater(this ITableClientFactory clientFactory) =>
        (series, token) =>
            clientFactory.GetClient<AnimeInfoStorage>().Bind(client => client.UpdateSeries(series, token));


    public static TvSeriesRemover TableStorageTvSeriesRemover(this ITableClientFactory clientFactory) =>
        (id, seasonString, token) => clientFactory.GetClient<AnimeInfoStorage>()
            .Bind(client => client.RemoveSeries(id, seasonString, token));

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

    private static Task<Result<Unit>> RemoveSeries(
        this TableClient tableClient,
        string id,
        string seasonString,
        CancellationToken token) =>
        tableClient.TryExecute<AnimeInfoStorage>(client =>
                client.DeleteEntityAsync(seasonString, id, cancellationToken: token))
            .WithDefaultMap()
            .MapError(error => error
                .WithLogProperty("Id", id)
                .WithLogProperty("Season", seasonString)
                .WithOperationName(nameof(RemoveSeries)));
}