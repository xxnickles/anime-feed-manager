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
    public static LatestSeasonGetter LatestSeasonGetter(this ITableClientFactory clientFactory) =>
        cancellationToken => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => client.GetLatestSeason(cancellationToken));

    public static SeasonGetter SeasonGetter(this ITableClientFactory clientFactory) =>
        (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => client.GetSeason(season, cancellationToken));

    public static AllSeasonsGetter AllSeasonsGetter(this ITableClientFactory clientFactory) =>
        cancellationToken => clientFactory.GetClient<SeasonStorage>()
            .Bind(client => GetAllSeasons(client, cancellationToken))
            .Map(seasons => seasons.TransformToSeriesSeason());

    public static Latest4SeasonsGetter Latest4SeasonsGetter(this ITableClientFactory clientFactory) =>
        cancellationToken => clientFactory.GetClient<LatestSeasonsStorage>()
            .Bind(client => client.GetLast4Seasons(cancellationToken)
                .Map(seasons => TransformToSeriesSeason(seasons, client.Logger)));


    private static Task<Result<ImmutableList<SeasonStorage>>> GetAllSeasons(
        this AppTableClient tableClient,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
            client.QueryAsync<SeasonStorage>(
                storage => storage.PartitionKey == SeasonType.Season || storage.PartitionKey == SeasonType.Latest,
                cancellationToken: cancellationToken));
    }

    private static Task<Result<SeasonStorageData>> GetLatestSeason(this AppTableClient tableClient,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<SeasonStorage>(storage => storage.PartitionKey == SeasonType.Latest,
                    cancellationToken: cancellationToken))
            .Map<ImmutableList<SeasonStorage>, SeasonStorageData>(seasons =>
                !seasons.IsEmpty ? new LatestSeason(seasons.First()) : new NoMatch());
    }

    private static Task<Result<SeasonStorageData>> GetSeason(this AppTableClient tableClient,
        SeriesSeason season, CancellationToken cancellation = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<SeasonStorage>(
                    storage => storage.PartitionKey == SeasonType.Season &&
                               storage.RowKey == IdHelpers.GenerateAnimePartitionKey(season),
                    cancellationToken: cancellation))
            .Map<ImmutableList<SeasonStorage>, SeasonStorageData>(matches =>
            {
                if (matches.IsEmpty)
                    return new NoMatch();

                var match = matches.First();

                return (match.Latest, season.IsLatest) switch
                {
                    (true, true) => new NoUpdateRequired(),
                    (true, false) => new LatestSeason(match),
                    _ => match.Season == season.Season && match.Year == season.Year
                        ? new NoUpdateRequired()
                        : new ExistentSeason(match)
                };
            });
    }


    private static Task<Result<LatestSeasonsStorage>> GetLast4Seasons(
        this AppTableClient tableClient,
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