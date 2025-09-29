namespace AnimeFeedManager.Features.Seasons.Storage;

public delegate Task<Result<Unit>> SeasonUpdater(SeasonStorage seasonStorage, CancellationToken token = default);

public delegate Task<Result<Unit>> LastestSeasonsUpdater(LatestSeasonsStorage seasonsStorage,
    CancellationToken token = default);

public delegate Task<Result<Unit>> SeasonRemover(string partition, string id, CancellationToken token = default);

public static class SeasonStore
{
    public static SeasonUpdater TableStorageSeasonUpdater(this ITableClientFactory clientFactory) =>
        (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => client.UpsertSeason(season, cancellationToken));

    public static LastestSeasonsUpdater TableStorageLastestSeasonsUpdater(this ITableClientFactory clientFactory) =>
        (seasons, cancellationToken) => clientFactory.GetClient<LatestSeasonsStorage>()
            .Bind(client => client.UpsertLatestSeasons(seasons, cancellationToken));

    public static SeasonRemover TableStorageSeasonRemover(this ITableClientFactory clientFactory) =>
        (partitionKey, id, token) => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => client.RemoveSeason(partitionKey, id, token));


    private static Task<Result<Unit>> UpsertSeason(
        this TableClient tableClient,
        SeasonStorage seasonStorage,
        CancellationToken token = default)
    {
        return tableClient
            .TryExecute<SeasonStorage>(client => client.UpsertEntityAsync(seasonStorage, cancellationToken: token))
            .WithDefaultMap()
            .MapError(error => error
                .WithLogProperty(nameof(SeasonStorage), seasonStorage)
                .WithOperationName(nameof(UpsertSeason)));
    }

    private static Task<Result<Unit>> UpsertLatestSeasons(this TableClient tableClient,
        LatestSeasonsStorage latestSeasonsStorage, CancellationToken token = default)
    {
        return tableClient
            .TryExecute<LatestSeasonsStorage>(client =>
                client.UpsertEntityAsync(latestSeasonsStorage, cancellationToken: token))
            .WithDefaultMap()
            .MapError(error => error
                .WithLogProperty(nameof(LatestSeasonsStorage), latestSeasonsStorage)
                .WithOperationName(nameof(UpsertLatestSeasons)));
    }

    private static Task<Result<Unit>> RemoveSeason(this TableClient tableClient,
        string partitionKey, string rowKey, CancellationToken token = default)
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