namespace AnimeFeedManager.Features.Seasons.Storage.Stores;

public delegate Task<Result<Unit>> SeasonUpdater(SeasonStorage seasonStorage, CancellationToken token = default);

public delegate Task<Result<Unit>> LastestSeasonsUpdater(LatestSeasonsStorage seasonsStorage,
    CancellationToken token = default);

public delegate Task<Result<Unit>> SeasonRemover(string partition, string id, CancellationToken token = default);

public static class SeasonStore
{
    extension(ITableClientFactory clientFactory)
    {
        public SeasonUpdater TableStorageSeasonUpdater() =>
            (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>()
                .Bind(client => client.UpsertSeason(season, cancellationToken));

        public LastestSeasonsUpdater TableStorageLastestSeasonsUpdater() =>
            (seasons, cancellationToken) => clientFactory.GetClient<LatestSeasonsStorage>()
                .Bind(client => client.UpsertLatestSeasons(seasons, cancellationToken));

        public SeasonRemover TableStorageSeasonRemover() =>
            (partitionKey, id, token) => clientFactory.GetClient<SeasonStorage>()
                .Bind(client => client.RemoveSeason(partitionKey, id, token));
    }


    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> UpsertSeason(SeasonStorage seasonStorage,
            CancellationToken token = default)
        {
            return tableClient
                .TryExecute<SeasonStorage>(client => client.UpsertEntityAsync(seasonStorage, cancellationToken: token))
                .WithDefaultMap()
                .MapError(error => error
                    .WithLogProperty(nameof(SeasonStorage), seasonStorage)
                    .WithOperationName(nameof(UpsertSeason)));
        }

        private Task<Result<Unit>> UpsertLatestSeasons(LatestSeasonsStorage latestSeasonsStorage, CancellationToken token = default)
        {
            return tableClient
                .TryExecute<LatestSeasonsStorage>(client =>
                    client.UpsertEntityAsync(latestSeasonsStorage, cancellationToken: token))
                .WithDefaultMap()
                .MapError(error => error
                    .WithLogProperty(nameof(LatestSeasonsStorage), latestSeasonsStorage)
                    .WithOperationName(nameof(UpsertLatestSeasons)));
        }

        private Task<Result<Unit>> RemoveSeason(string partitionKey, string rowKey, CancellationToken token = default)
        {
            return tableClient
                .TryExecute<SeasonStorage>(client =>
                    client.DeleteEntityAsync(partitionKey, rowKey, cancellationToken: token))
                .WithDefaultMap()
                .MapError(error => error
                    .WithLogProperty(nameof(SeasonStorage.PartitionKey), partitionKey)
                    .WithLogProperty(nameof(SeasonStorage.RowKey), rowKey)
                    .WithOperationName(nameof(RemoveSeason)));
        }
    }
}