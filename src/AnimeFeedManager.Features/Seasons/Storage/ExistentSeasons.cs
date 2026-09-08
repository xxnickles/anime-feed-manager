using AnimeFeedManager.Features.Seasons.UpdateProcess;
using CommonJsonContext = AnimeFeedManager.Features.Common.CommonJsonContext;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Seasons.Storage;

public delegate Task<Result<SeasonStorageData>> LatestSeasonGetter(CancellationToken cancellationToken = default);

public delegate Task<Result<SeasonStorageData>> SeasonGetter(SeriesSeason season,
    CancellationToken cancellation = default);

public delegate Task<Result<ImmutableArray<SeriesSeason>>> AllSeasonsGetter(
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<SeriesSeason>>> Latest4SeasonsGetter(
    CancellationToken cancellationToken = default);

public static class ExistentSeasons
{
    extension(ITableClientFactory clientFactory)
    {
        // Election is a deliberate admin action, so nothing may carry the flag. Rather than leave the
        // app featureless, the newest known season stands in — tagged so the promotion path can tell
        // the two apart.
        public LatestSeasonGetter TableStorageLatestSeason =>
            cancellationToken => clientFactory.GetClient<SeasonStorage>()
                .WithOperationName("TableStorageLatestSeason")
                .Bind(client => client
                    .ExecuteQuery<SeasonStorage>(
                        storage => storage.PartitionKey == SeasonStorage.SeasonPartition,
                        cancellationToken)
                    .Map<ImmutableArray<SeasonStorage>, SeasonStorageData>(seasons =>
                        seasons.FirstOrDefault(season => season.Latest) is { } flagged
                            ? new CurrentLatestSeason(flagged)
                            : NewestSeason(seasons)));

        public SeasonGetter TableStorageSeason =>
            (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>()
                .WithOperationName("TableStorageSeason")
                .WithLogProperty(nameof(SeriesSeason), season)
                .Bind(client => client.ExecuteQuery<SeasonStorage>(storage =>
                        storage.PartitionKey == SeasonStorage.SeasonPartition &&
                        storage.RowKey == IdHelpers.GenerateAnimePartitionKey(season), cancellationToken)
                    .Map<ImmutableArray<SeasonStorage>, SeasonStorageData>(matches =>
                    {
                        if (matches.IsEmpty)
                            return new NoMatch();

                        var match = matches[0];

                        return (match.Latest, season.IsLatest) switch
                        {
                            (true, true)
                                or (true, false) =>
                                new NoUpdateRequired(), // Do not update if is the current latest season
                            (false, true) => UpdateCurrentToLatest(match),
                            _ => match.Season == season.Season && match.Year == season.Year
                                ? new NoUpdateRequired()
                                : new ExistentSeason(match)
                        };
                    }));

        public AllSeasonsGetter TableStorageAllSeasonsGetter =>
            cancellationToken => clientFactory.GetClient<SeasonStorage>()
                .WithOperationName("AllSeasonsGetter")
                .Bind(client =>
                    client.ExecuteQuery<SeasonStorage>(storage => storage.PartitionKey == SeasonStorage.SeasonPartition,
                        cancellationToken))
                .Map(seasons => seasons.TransformToSeriesSeason());

        public Latest4SeasonsGetter TableStorageLatest4SeasonsGetter(ILogger logger) =>
            cancellationToken => clientFactory.GetClient<LatestSeasonsStorage>()
                .WithOperationName(nameof(TableStorageLatest4SeasonsGetter))
                .WithLogProperties([
                    new KeyValuePair<string, object>(nameof(LatestSeasonsStorage.Partition),
                        LatestSeasonsStorage.Partition),
                    new KeyValuePair<string, object>(nameof(LatestSeasonsStorage.RowKey), LatestSeasonsStorage.Key)
                ])
                .Bind(client => client.ExecuteQuery<LatestSeasonsStorage>(storage =>
                        storage.PartitionKey == LatestSeasonsStorage.Partition &&
                        storage.RowKey == LatestSeasonsStorage.Key, cancellationToken)
                    .SingleItem()
                )
                .Map(seasons => seasons.TransformToSeriesSeason(logger));
    }

    private static SeasonStorageData NewestSeason(ImmutableArray<SeasonStorage> seasons) =>
        seasons
            .Where(season => Season.IsValid(season.Season ?? string.Empty))
            .OrderByDescending(season => season.Year)
            .ThenByDescending(season => Season.FromString(season.Season))
            .FirstOrDefault() is { } newest
            ? new FallbackLatestSeason(newest)
            : new NoMatch();

    private static ReplaceLatestSeason UpdateCurrentToLatest(SeasonStorage storage)
    {
        storage.Latest = true;
        return new ReplaceLatestSeason(storage);
    }

    private static ImmutableArray<SeriesSeason> TransformToSeriesSeason(this LatestSeasonsStorage seasons,
        ILogger logger)
    {
        try
        {
            return (JsonSerializer.Deserialize(seasons.Payload ?? string.Empty,
                CommonJsonContext.Default.SeriesSeasonArray) ?? []).ToImmutableArray();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "An error occurred when trying to deserialize the latest seasons");
            return [];
        }
    }

    private static ImmutableArray<SeriesSeason> TransformToSeriesSeason(this ImmutableArray<SeasonStorage> seasons) =>
        seasons.Select(s => (s.Season ?? string.Empty, s.Year, s.Latest).ParseAsSeriesSeason())
            .Flatten(list => list.ToImmutableArray())
            .MatchToValue(bulk => bulk.Value, _ => ImmutableArray<SeriesSeason>.Empty);
}