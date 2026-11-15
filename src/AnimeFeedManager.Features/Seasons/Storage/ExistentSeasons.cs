using AnimeFeedManager.Features.Seasons.UpdateProcess;
using CommonJsonContext = AnimeFeedManager.Features.Common.CommonJsonContext;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Seasons.Storage;

public delegate Task<Result<LatestSeasonRead, StorageError>> LatestSeasonGetter(CancellationToken cancellationToken = default);

public delegate Task<Result<SeasonRead, StorageError>> SeasonGetter(SeriesSeason season,
    CancellationToken cancellation = default);

public delegate Task<Result<ImmutableArray<SeriesSeason>, StorageError>> AllSeasonsGetter(
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<SeriesSeason>, StorageError>> Latest4SeasonsGetter(
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
                    .Map(LatestSeasonRead (seasons) =>
                        seasons.FirstOrDefault(season => season.Latest) is { } flagged
                            ? new CurrentLatestSeason(flagged)
                            : NewestSeason(seasons)));

        public SeasonGetter TableStorageSeason =>
            (season, cancellationToken) => clientFactory.GetClient<SeasonStorage>()
                .WithOperationName("TableStorageSeason")
                .WithLogProperty(Telemetry.SeriesSeasonTag, IdHelpers.GenerateAnimePartitionKey(season))
                .Bind(client => client.ExecuteQuery<SeasonStorage>(storage =>
                        storage.PartitionKey == SeasonStorage.SeasonPartition &&
                        storage.RowKey == IdHelpers.GenerateAnimePartitionKey(season), cancellationToken)
                    .Map(SeasonRead (matches) =>
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
                .Bind(seasons => seasons.TransformToSeriesSeason());

        public Latest4SeasonsGetter TableStorageLatest4SeasonsGetter =>
            cancellationToken => clientFactory.GetClient<LatestSeasonsStorage>()
                .WithOperationName("TableStorageLatest4SeasonsGetter")
                .Bind(client => client.ExecuteQuery<LatestSeasonsStorage>(storage =>
                        storage.PartitionKey == LatestSeasonsStorage.Partition &&
                        storage.RowKey == LatestSeasonsStorage.Key, cancellationToken)
                    .SingleItem()
                )
                .Bind(seasons => DeserializeSeasons(seasons.Payload));
    }

    private static LatestSeasonRead NewestSeason(ImmutableArray<SeasonStorage> seasons) =>
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

    private static Result<ImmutableArray<SeriesSeason>, StorageError> DeserializeSeasons(string? payload)
    {
        try
        {
            return JsonSerializer.Deserialize(payload ?? string.Empty, CommonJsonContext.Default.SeriesSeasonArray) is { } seasons
                ? seasons.ToImmutableArray()
                : new StorageError.UnreadablePayload(typeof(LatestSeasonsStorage), null);
        }
        catch (JsonException e)
        {
            return new StorageError.UnreadablePayload(typeof(LatestSeasonsStorage), e);
        }
    }

    // Unreadable rows are logged and left out.
    private static Result<ImmutableArray<SeriesSeason>, StorageError> TransformToSeriesSeason(
        this ImmutableArray<SeasonStorage> seasons) =>
        seasons.Select(s => (s.Season ?? string.Empty, s.Year, s.Latest).ParseAsSeriesSeason()
                .MapError(StorageError (fields) => new StorageError.InvalidRecord(typeof(SeasonStorage), fields.Errors)))
            .CollectSuccesses(list => list.ToImmutableArray());
}