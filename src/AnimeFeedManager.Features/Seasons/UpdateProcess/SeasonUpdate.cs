namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public static class SeasonUpdate
{
    public static Task<Result<SeasonUpdateData>> CheckSeasonExist(SeasonGetter seasonGetter, SeriesSeason season,
        CancellationToken token) =>
        seasonGetter(season, token)
            .Map(s => new SeasonUpdateData(season, s, new NoMatch()));


    extension(Task<Result<SeasonUpdateData>> processData)
    {
        public Task<Result<SeasonUpdateData>> CreateNewSeason() =>
            processData.Map(data => data.SeasonData switch
            {
                NoMatch => CreateNewSeason(data),
                _ => data
            });

        public Task<Result<SeasonUpdateData>> AddLatestSeasonData(LatestSeasonGetter seasonGetter, CancellationToken token) =>
            processData.BindWhen(
                data => seasonGetter(token).Map(latestSeason => data with {CurrentLatestSeasonData = latestSeason}),
                WhenNeedsUpdate);

        public Task<Result<SeasonUpdateData>> StoreUpdatedSeason(SeasonUpdater seasonUpdater, CancellationToken token) =>
            processData.BindWhen(data => UpdateSeason(seasonUpdater, data.SeasonData, token).Map(_ => data),
                WhenNeedsUpdate);

        public Task<Result<SeasonUpdateData>> DemoteCurrentLatest(SeasonUpdater seasonUpdater, CancellationToken token) =>
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

    private static Task<Result<Unit>> UpdateSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) => data switch
    {
        ExistentSeason existent => seasonUpdater(existent.Season, cancellationToken),
        CurrentLatestSeason latest => seasonUpdater(latest.Season, cancellationToken),
        NewSeason newSeason => seasonUpdater(newSeason.Season, cancellationToken),
        ReplaceLatestSeason newLatest => seasonUpdater(newLatest.Season, cancellationToken),
        _ => Task.FromResult<Result<Unit>>(new OperationError(
            $"{nameof(UpdateSeason)}-{nameof(SeasonStorageData)}",
            $"Season data is not selectable to be updated. Received {data.GetType().Name}"))
    };

    private static Task<Result<Unit>> UpdateCurrentLatestSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) => data switch
    {
        CurrentLatestSeason latest => seasonUpdater(DemoteCurrentLatestSeason(latest.Season), cancellationToken),
        _ => Task.FromResult<Result<Unit>>(new OperationError(
            $"{nameof(UpdateCurrentLatestSeason)}-{nameof(SeasonStorageData)}",
            $"Season data is not selectable to be updated as is not the current lastest season. Received {data.GetType().Name}"))
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