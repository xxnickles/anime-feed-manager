using AnimeFeedManager.Features.Seasons.UpdateProcess;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;
using SeriesSeasonContext = AnimeFeedManager.Features.Common.SeriesSeasonContext;

namespace AnimeFeedManager.Features.Seasons.Storage;

public delegate Task<Result<SeasonStorageData>> LatestSeasonGetter(CancellationToken cancellationToken = default);

public delegate Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason season,
    CancellationToken cancellation = default);

public delegate Task<Result<ImmutableList<SeriesSeason>>> AllSeasonsGetter(
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<SeriesSeason>>> Latest4SeasonsGetter(
    CancellationToken cancellationToken = default);

public static class ExistentSeasons
{
    public static LatestSeasonGetter TableStorageLatestSeasonGetter(this ITableClientFactory clientFactory) =>
        cancellationToken => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => client.GetLatestSeason(cancellationToken));

    public static SeasonGetter TableStorageSeasonGetter(this ITableClientFactory clientFactory) =>
        (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => client.GetSeason(season, cancellationToken));

    public static AllSeasonsGetter TableStorageAllSeasonsGetter(this ITableClientFactory clientFactory) =>
        cancellationToken => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => GetAllSeasons(client, cancellationToken))
            .Map(seasons => seasons.TransformToSeriesSeason());

    public static Latest4SeasonsGetter TableStorageLatest4SeasonsGetter(this ITableClientFactory clientFactory, ILogger logger) =>
        cancellationToken => clientFactory.GetClient<LatestSeasonsStorage>()
            .Bind(client => client.GetLast4Seasons(cancellationToken)
                .Map(seasons => TransformToSeriesSeason(seasons, logger)));


    private static Task<Result<ImmutableList<SeasonStorage>>> GetAllSeasons(
        this TableClient tableClient,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
            client.QueryAsync<SeasonStorage>(
                storage => storage.PartitionKey == SeasonStorage.SeasonPartition,
                cancellationToken: cancellationToken));
    }

    private static Task<Result<SeasonStorageData>> GetLatestSeason(this TableClient tableClient,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<SeasonStorage>(storage => storage.PartitionKey == SeasonStorage.SeasonPartition && storage.Latest == true,
                    cancellationToken: cancellationToken))
            .Map<ImmutableList<SeasonStorage>, SeasonStorageData>(seasons =>
                !seasons.IsEmpty ? new CurrentLatestSeason(seasons.First()) : new NoMatch());
    }

    private static Task<Result<SeasonStorageData>> GetSeason(this TableClient tableClient,
        SeriesSeason season, CancellationToken cancellation = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<SeasonStorage>(
                    storage => storage.PartitionKey == SeasonStorage.SeasonPartition &&
                               storage.RowKey == IdHelpers.GenerateAnimePartitionKey(season),
                    cancellationToken: cancellation))
            .Map<ImmutableList<SeasonStorage>, SeasonStorageData>(matches =>
            {
                if (matches.IsEmpty)
                    return new NoMatch();

                var match = matches.First();

                return (match.Latest, season.IsLatest) switch
                {
                    (true, true) or (true, false) => new NoUpdateRequired(), // Do not update if is the current latest season
                    (false, true) => UpdateCurrentToLatest(match),
                    _ => match.Season == season.Season && match.Year == season.Year
                        ? new NoUpdateRequired()
                        : new ExistentSeason(match)
                };
            });
    }
    
    private static ReplaceLatestSeason UpdateCurrentToLatest(SeasonStorage storage)
    {
        storage.Latest = true;
        return new ReplaceLatestSeason(storage);
    }


    private static Task<Result<LatestSeasonsStorage>> GetLast4Seasons(
        this TableClient tableClient,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<LatestSeasonsStorage>(
                    storage => storage.PartitionKey == LatestSeasonsStorage.Partition &&
                               storage.RowKey == LatestSeasonsStorage.Key, cancellationToken: cancellationToken))
            .SingleItem();
    }

    private static ImmutableList<SeriesSeason> TransformToSeriesSeason(this LatestSeasonsStorage seasons,
        ILogger logger)
    {
        try
        {
            return (JsonSerializer.Deserialize(seasons.Payload ?? string.Empty,
                SeriesSeasonContext.Default.SeriesSeasonArray) ?? []).ToImmutableList();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "An error occurred when trying to deserialize the latest seasons");
            return [];
        }
    }


    private static ImmutableList<SeriesSeason> TransformToSeriesSeason(this ImmutableList<SeasonStorage> seasons) =>
        seasons.ConvertAll(s => (s.Season ?? string.Empty, s.Year, s.Latest).ParseAsSeriesSeason())
            .GetSuccessValues();
}