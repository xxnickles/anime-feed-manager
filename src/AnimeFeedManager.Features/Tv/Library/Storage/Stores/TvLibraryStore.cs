namespace AnimeFeedManager.Features.Tv.Library.Storage.Stores;

public delegate Task<Result<Unit>> TvLibraryStorageUpdater(IEnumerable<AnimeInfoStorage> series,
    CancellationToken cancellationToken = default);

public delegate Task<Result<Unit>> TvSeriesStorageUpdater(AnimeInfoStorage series,
    CancellationToken cancellationToken = default);

public delegate Task<Result<Unit>> TvSeriesRemover(string id, string seasonString, CancellationToken token = default);

public static class TvLibraryStore
{
    extension(ITableClientFactory clientFactory)
    {
        public TvLibraryStorageUpdater TableStorageTvLibraryUpdater =>
            (series, token) =>
                clientFactory.GetClient<AnimeInfoStorage>()
                    .Bind(client => client.UpsertSeries(series, token));

        public TvSeriesStorageUpdater TableStorageTvSeriesUpdater =>
            (series, token) =>
                clientFactory.GetClient<AnimeInfoStorage>().Bind(client => client.UpdateSeries(series, token));

        public TvSeriesRemover TableStorageTvSeriesRemover =>
            (id, seasonString, token) => clientFactory.GetClient<AnimeInfoStorage>()
                .Bind(client => client.RemoveSeries(id, seasonString, token));
    }


    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> UpsertSeries(IEnumerable<AnimeInfoStorage> series,
            CancellationToken cancellationToken = default)
        {
            return tableClient.AddBatch(series, cancellationToken)
                .MapError(error => error
                    .WithLogProperty("Count", series.Count())
                    .WithOperationName(nameof(UpsertSeries)));
        }

        private Task<Result<Unit>> UpdateSeries(AnimeInfoStorage series,
            CancellationToken token) =>
            tableClient.TryExecute<AnimeInfoStorage>(client =>
                    client.UpdateEntityAsync(series, series.ETag, cancellationToken: token))
                .WithDefaultMap()
                .MapError(error => error
                    .WithLogProperty("Series", series)
                    .WithOperationName(nameof(UpdateSeries)));

        private Task<Result<Unit>> RemoveSeries(string id,
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
}