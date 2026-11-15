namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

[Widens]
public partial union SeasonUpdateError(StorageError, SeasonNotUpdatable, DeliveryError);

public static class SeasonUpdate
{
    public static Task<Result<SeasonUpdateData, SeasonUpdateError>> CheckSeasonExist(SeasonGetter seasonGetter, SeriesSeason season,
        CancellationToken token) =>
        seasonGetter(season, token)
            .Map(s => new SeasonUpdateData(season, s, new NoMatch()))
            .MapError(error => (SeasonUpdateError)error);


    extension(Task<Result<SeasonUpdateData, SeasonUpdateError>> processData)
    {
        public Task<Result<SeasonUpdateData, SeasonUpdateError>> CreateNewSeason() =>
            processData.Map(data => data.SeasonData switch
            {
                NoMatch => CreateNewSeason(data),
                _ => data
            });

        public Task<Result<SeasonUpdateData, SeasonUpdateError>> AddLatestSeasonData(LatestSeasonGetter seasonGetter, CancellationToken token) =>
            processData.BindWhen(
                data => seasonGetter(token).Map(latestSeason => data with {CurrentLatestSeasonData = latestSeason}),
                WhenNeedsUpdate);

        public Task<Result<SeasonUpdateData, SeasonUpdateError>> StoreUpdatedSeason(SeasonUpdater seasonUpdater, CancellationToken token) =>
            processData.BindWhen(data => UpdateSeason(seasonUpdater, data.SeasonData, token).Map(_ => data),
                WhenNeedsUpdate);

        public Task<Result<SeasonUpdateData, SeasonUpdateError>> DemoteCurrentLatest(SeasonUpdater seasonUpdater, CancellationToken token) =>
            processData.BindWhen(
                data => UpdateCurrentLatestSeason(seasonUpdater, data.CurrentLatestSeasonData, token).Map(_ => data),
                WhenLastestIsReplaced);
    }


    private static SeasonUpdateData CreateNewSeason(SeasonUpdateData data)
    {
        var newSeason = new SeasonStorage
        {
            RowKey = IdHelpers.GenerateAnimePartitionKey(data.SeasonToUpdate),
            PartitionKey = SeasonStorage.SeasonPartition,
            Latest = data.SeasonToUpdate.IsLatest,
            Season = data.SeasonToUpdate.Season,
            Year = data.SeasonToUpdate.Year
        };

        return data with
        {
            SeasonData = new NewSeason(newSeason)
        };
    }

    private static Task<Result<Unit, SeasonUpdateError>> UpdateSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) =>
        SelectableSeason(data).MapError(error => (SeasonUpdateError)error)
            .Bind(season => seasonUpdater(season, cancellationToken));

    private static Result<SeasonStorage, SeasonNotUpdatable> SelectableSeason(SeasonStorageData data) => data switch
    {
        ExistentSeason existent => existent.Season,
        CurrentLatestSeason latest => latest.Season,
        NewSeason newSeason => newSeason.Season,
        ReplaceLatestSeason newLatest => newLatest.Season,
        _ => new SeasonNotUpdatable(data.GetType(), "an existent, new or latest season")
    };

    private static Task<Result<Unit, SeasonUpdateError>> UpdateCurrentLatestSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) =>
        DemotableSeason(data).MapError(error => (SeasonUpdateError)error)
            .Bind(season => seasonUpdater(season, cancellationToken));

    private static Result<SeasonStorage, SeasonNotUpdatable> DemotableSeason(SeasonStorageData data) => data switch
    {
        CurrentLatestSeason latest => DemoteCurrentLatestSeason(latest.Season),
        _ => new SeasonNotUpdatable(data.GetType(), "the current latest season")
    };

    private static SeasonStorage DemoteCurrentLatestSeason(SeasonStorage data)
    {
        data.Latest = false;
        return data;
    }

    private static bool WhenNeedsUpdate(SeasonUpdateData data) =>
        data.SeasonData is not NoUpdateRequired;

    private static bool WhenLastestIsReplaced(SeasonUpdateData data) =>
        (data.SeasonData, LatestSeasonData: data.CurrentLatestSeasonData) switch
        {
            (ReplaceLatestSeason or NewSeason {Season.Latest: true}, CurrentLatestSeason) => true,
            _ => false,
        };
}