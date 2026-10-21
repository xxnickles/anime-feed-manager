namespace AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

public delegate Task<Result<Unit>> NyaaCatalogWiper(string seriesId, CancellationToken token = default);

/// <summary>Upsert-Replace: the info hash in the row key makes re-polling the same release idempotent.</summary>
public delegate Task<Result<Unit>> NyaaCatalogUpdater(IEnumerable<NyaaCatalogStorage> rows,
    CancellationToken token = default);

public delegate Task<Result<ImmutableArray<NyaaCatalogStorage>>> NyaaCatalogGetter(string seriesId,
    CancellationToken token = default);

public static class NyaaCatalogStore
{
    extension(ITableClientFactory clientFactory)
    {
        public NyaaCatalogWiper TableStorageNyaaCatalogWiper =>
            (seriesId, token) => clientFactory.GetClient<NyaaCatalogStorage>()
                .Bind(client => client.WipeCatalog(seriesId, token));

        public NyaaCatalogUpdater TableStorageNyaaCatalogUpdater =>
            (rows, token) => clientFactory.GetClient<NyaaCatalogStorage>()
                .Bind(client => client.UpsertRows(rows, token));

        public NyaaCatalogGetter TableStorageNyaaCatalogGetter =>
            (seriesId, token) => clientFactory.GetClient<NyaaCatalogStorage>()
                .WithOperationName("TableStorageNyaaCatalogGetter")
                .WithLogProperty(Telemetry.SeriesIdTag, seriesId)
                .Bind(client => client.ExecuteQuery<NyaaCatalogStorage>(row => row.PartitionKey == seriesId, token));
    }

    extension(TableClient tableClient)
    {
        // Keys only, deleted with ETag.All: the wipe must not lose to a concurrent upsert of the same row.
        private Task<Result<Unit>> WipeCatalog(string seriesId, CancellationToken token) =>
            tableClient.ExecuteQuery<NyaaCatalogStorage>(row => row.PartitionKey == seriesId, token,
                    [nameof(NyaaCatalogStorage.PartitionKey), nameof(NyaaCatalogStorage.RowKey)])
                .Bind(rows => tableClient.AddBatch(
                    rows.Select(row => new NyaaCatalogStorage
                        {PartitionKey = row.PartitionKey, RowKey = row.RowKey, ETag = ETag.All}),
                    token,
                    TableTransactionActionType.Delete))
                .WithOperationName(nameof(WipeCatalog))
                .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

        private Task<Result<Unit>> UpsertRows(IEnumerable<NyaaCatalogStorage> rows, CancellationToken token) =>
            tableClient.AddBatch(rows, token, TableTransactionActionType.UpsertReplace)
                .WithOperationName(nameof(UpsertRows));
    }
}
