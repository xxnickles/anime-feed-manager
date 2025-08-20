namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public static class SeasonUpdate
{
    public static Task<Result<SeasonUpdateData>> CheckSeasonExist(SeasonGetter seasonGetter, SeriesSeason season,
        CancellationToken token) =>
        seasonGetter(season, token)
            .Map(s => new SeasonUpdateData(season, s, s is not LatestSeason ? new NoMatch() : s));


    public static Task<Result<SeasonUpdateData>> CreateNewSeason(this Task<Result<SeasonUpdateData>> processData) =>
        processData.Map(data => data.SeasonData switch
        {
            NoMatch => CreateNewSeason(data),
            _ => data
        });

    public static Task<Result<SeasonUpdateData>> AddLatestSeasonData(this Task<Result<SeasonUpdateData>> processData,
        LatestSeasonGetter seasonGetter, CancellationToken token) =>
        processData.BindSeasonData(
            data => seasonGetter(token).Map(latestSeason => data with {LatestSeasonData = latestSeason}),
            WhenNewIsNotLatest);

    public static Task<Result<SeasonUpdateData>> StoreUpdatedSeason(this Task<Result<SeasonUpdateData>> processData,
        SeasonUpdater seasonUpdater, CancellationToken token) =>
        processData.BindSeasonData(data => UpdateSeason(seasonUpdater, data.SeasonData, token).Map(_ => data),
            WhenCanUpdate);

    public static Task<Result<SeasonUpdateData>> DemoteCurrentLatest(this Task<Result<SeasonUpdateData>> processData,
        SeasonUpdater seasonUpdater, CancellationToken token) =>
        processData.BindSeasonData(
            data => UpdateLatestSeason(seasonUpdater, data.LatestSeasonData, token).Map(_ => data),
            WhenLastestIsReplaced);


    private static SeasonUpdateData CreateNewSeason(SeasonUpdateData data)
    {
        var newSeason = new SeasonStorage
        {
            RowKey = IdHelpers.GenerateAnimePartitionKey(data.SeasonToUpdate),
            PartitionKey = data.SeasonToUpdate.IsLatest ? SeasonType.Latest : SeasonType.Season,
            Latest = data.SeasonToUpdate.IsLatest,
            Season = data.SeasonToUpdate.Season,
            Year = data.SeasonToUpdate.Year
        };

        return data with
        {
            SeasonData = data.SeasonToUpdate.IsLatest ? new LatestSeason(newSeason) : new NewSeason(newSeason)
        };
    }

    private static Task<Result<Unit>> UpdateSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) => data switch
    {
        ExistentSeason existent => seasonUpdater(existent.Season, cancellationToken),
        LatestSeason latest => seasonUpdater(latest.Season, cancellationToken),
        NewSeason newSeason => seasonUpdater(newSeason.Season, cancellationToken),
        _ => Task.FromResult<Result<Unit>>(new OperationError(
            $"{nameof(UpdateSeason)}-{nameof(SeasonStorageData)}",
            $"Season data is not selectable to be updated. Received {data.GetType().Name}"))
    };

    private static Task<Result<Unit>> UpdateLatestSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) => data switch
    {
        LatestSeason latest => seasonUpdater(DemoteCurrentLatestSeason(latest.Season), cancellationToken),
        _ => Task.FromResult<Result<Unit>>(new OperationError(
            $"{nameof(UpdateLatestSeason)}-{nameof(SeasonStorageData)}",
            $"Season data is not selectable to be  updated. Received {data.GetType().Name}"))
    };

    private static SeasonStorage DemoteCurrentLatestSeason(SeasonStorage data)
    {
        data.Latest = false;
        return data;
    }

    private static bool WhenIsANewSeason(SeasonUpdateData data) =>
        data.SeasonData is NoMatch;

    private static bool WhenNewIsNotLatest(SeasonUpdateData data) =>
        data.SeasonData is not NoUpdateRequired && data.LatestSeasonData is not LatestSeason;

    private static bool WhenCanUpdate(SeasonUpdateData data) =>
        data.SeasonData is not NoUpdateRequired;

    private static bool WhenLastestIsReplaced(SeasonUpdateData data) => (data.SeasonData, data.LatestSeasonData) switch
    {
        (LatestSeason updatedSeason, LatestSeason currentLatest) => !Utils.IsSameSeasonData(updatedSeason,
            currentLatest),
        _ => false,
    };
}