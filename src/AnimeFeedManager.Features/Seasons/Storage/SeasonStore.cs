namespace AnimeFeedManager.Features.Seasons.Storage;

public delegate Task<Result<Unit>> SeasonUpdater(SeasonStorage seasonStorage, CancellationToken token = default);

public delegate Task<Result<Unit>> LastestSeasonsUpdater(LatestSeasonsStorage seasonsStorage,
    CancellationToken token = default);

public static class SeasonStore
{
    public static SeasonUpdater SeasonUpdater(this ITableClientFactory clientFactory) =>
        (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>(cancellationToken)
            .Bind(client => client.UpsertSeason(season, cancellationToken));

    public static LastestSeasonsUpdater LastestSeasonsUpdater(this ITableClientFactory clientFactory) =>
        (seasons, cancellationToken) => clientFactory.GetClient<LatestSeasonsStorage>(cancellationToken)
            .Bind(client => client.UpsertLatestSeasons(seasons, cancellationToken));


    private static Task<Result<Unit>> UpsertSeason(
        this AppTableClient<SeasonStorage> tableClient,
        SeasonStorage seasonStorage,
        CancellationToken token = default)
    {
        return tableClient
            .TryExecute(client => client.UpsertEntityAsync(seasonStorage, cancellationToken: token))
            .WithDefaultMap();
    }
    
    private static Task<Result<Unit>> UpsertLatestSeasons(this AppTableClient<LatestSeasonsStorage> tableClient,
        LatestSeasonsStorage latestSeasonsStorage, CancellationToken token = default)
    {
        return tableClient
            .TryExecute(client => client.UpsertEntityAsync(latestSeasonsStorage, cancellationToken: token))
            .WithDefaultMap();
    }
}