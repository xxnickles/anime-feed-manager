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
                .WithOperationName(nameof(UpsertSeries))
                .WithLogProperty("Count", series.Count());
        }

        private Task<Result<Unit>> UpdateSeries(AnimeInfoStorage series,
            CancellationToken token) =>
            tableClient.TryExecute<AnimeInfoStorage>(client =>
                    client.UpdateEntityAsync(series, series.ETag, cancellationToken: token))
                .WithDefaultMap()
                .WithOperationName(nameof(UpdateSeries))
                .WithLogProperty("Series", series);

        private Task<Result<Unit>> RemoveSeries(string id,
            string seasonString,
            CancellationToken token) =>
            tableClient.TryExecute<AnimeInfoStorage>(client =>
                    client.DeleteEntityAsync(seasonString, id, cancellationToken: token))
                .WithDefaultMap()
                .WithOperationName(nameof(RemoveSeries))
                .WithLogProperties([
                    new KeyValuePair<string, object>("Id", id),
                    new KeyValuePair<string, object>("Season", seasonString)
                ]);
    }
}