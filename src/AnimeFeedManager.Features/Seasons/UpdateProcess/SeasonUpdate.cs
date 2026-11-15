namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

[Widens]
public partial union SeasonUpdateError(StorageError, DeliveryError);

public sealed record SeasonCheck(SeriesSeason SeasonToUpdate, SeasonRead Read);

public static class SeasonUpdate
{
    public static Task<Result<SeasonCheck, SeasonUpdateError>> CheckSeasonExist(SeasonGetter seasonGetter, SeriesSeason season,
        CancellationToken token) =>
        seasonGetter(season, token)
            .Map(read => new SeasonCheck(season, read))
            .MapError(error => (SeasonUpdateError)error);

    extension(Task<Result<SeasonCheck, SeasonUpdateError>> check)
    {
        public Task<Result<SeasonUpdateData, SeasonUpdateError>> CreateNewSeason() =>
            check.Map(data => new SeasonUpdateData(data.SeasonToUpdate, AsPlan(data), new NoMatch()));
    }

    extension(Task<Result<SeasonUpdateData, SeasonUpdateError>> processData)
    {
        public Task<Result<SeasonUpdateData, SeasonUpdateError>> AddLatestSeasonData(LatestSeasonGetter seasonGetter, CancellationToken token) =>
            processData.BindWhen(
                data => seasonGetter(token).Map(latest => data with {Latest = latest}),
                Promotes);

        public Task<Result<SeasonUpdateData, SeasonUpdateError>> StoreUpdatedSeason(SeasonUpdater seasonUpdater, CancellationToken token) =>
            processData.Bind(data => (data.Plan switch
            {
                NoUpdateRequired => Task.FromResult(Result<Unit, StorageError>.Success(new Unit())),
                ExistentSeason existent => seasonUpdater(existent.Season, token),
                ReplaceLatestSeason replace => seasonUpdater(replace.Season, token),
                NewSeason created => seasonUpdater(created.Season, token)
            }).Map(_ => data));

        public Task<Result<SeasonUpdateData, SeasonUpdateError>> DemoteCurrentLatest(SeasonUpdater seasonUpdater, CancellationToken token) =>
            processData.Bind(data => (data.Latest switch
            {
                CurrentLatestSeason current when Promotes(data) => seasonUpdater(Demoted(current.Season), token),
                _ => Task.FromResult(Result<Unit, StorageError>.Success(new Unit()))
            }).Map(_ => data));
    }

    private static SeasonPlan AsPlan(SeasonCheck check) => check.Read switch
    {
        NoMatch => new NewSeason(NewSeasonStorage(check.SeasonToUpdate)),
        NoUpdateRequired skip => skip,
        ExistentSeason existent => existent,
        ReplaceLatestSeason replace => replace
    };

    private static SeasonStorage NewSeasonStorage(SeriesSeason season) => new()
    {
        RowKey = IdHelpers.GenerateAnimePartitionKey(season),
        PartitionKey = SeasonStorage.SeasonPartition,
        Latest = season.IsLatest,
        Season = season.Season,
        Year = season.Year
    };

    private static SeasonStorage Demoted(SeasonStorage season)
    {
        season.Latest = false;
        return season;
    }

    private static bool Promotes(SeasonUpdateData data) =>
        data.Plan is ReplaceLatestSeason or NewSeason {Season.Latest: true};
}
