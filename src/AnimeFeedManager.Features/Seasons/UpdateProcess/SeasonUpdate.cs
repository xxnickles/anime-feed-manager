namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public static class SeasonUpdate
{
    public static Task<Result<SeasonUpdateData, DomainError>> CheckSeasonExist(SeasonGetter seasonGetter, SeriesSeason season,
        CancellationToken token) =>
        seasonGetter(season, token)
            .Map(s => new SeasonUpdateData(season, s, new NoMatch()))
            .MapError(error => (DomainError)error);


    extension(Task<Result<SeasonUpdateData, DomainError>> processData)
    {
        public Task<Result<SeasonUpdateData, DomainError>> CreateNewSeason() =>
            processData.Map(data => data.SeasonData switch
            {
                NoMatch => CreateNewSeason(data),
                _ => data
            });

        public Task<Result<SeasonUpdateData, DomainError>> AddLatestSeasonData(LatestSeasonGetter seasonGetter, CancellationToken token) =>
            processData.BindWhen(
                data => seasonGetter(token).Map(latestSeason => data with {CurrentLatestSeasonData = latestSeason}),
                WhenNeedsUpdate);

        public Task<Result<SeasonUpdateData, DomainError>> StoreUpdatedSeason(SeasonUpdater seasonUpdater, CancellationToken token) =>
            processData.BindWhen(data => UpdateSeason(seasonUpdater, data.SeasonData, token).Map(_ => data),
                WhenNeedsUpdate);

        public Task<Result<SeasonUpdateData, DomainError>> DemoteCurrentLatest(SeasonUpdater seasonUpdater, CancellationToken token) =>
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

    private static Task<Result<Unit, DomainError>> UpdateSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) =>
        SelectableSeason(data).Bind(season => seasonUpdater(season, cancellationToken));

    private static Result<SeasonStorage, DomainError> SelectableSeason(SeasonStorageData data) => data switch
    {
        ExistentSeason existent => existent.Season,
        CurrentLatestSeason latest => latest.Season,
        NewSeason newSeason => newSeason.Season,
        ReplaceLatestSeason newLatest => newLatest.Season,
        _ => Error.Create($"Season data is not selectable to be updated. Received {data.GetType().Name}")
    };

    private static Task<Result<Unit, DomainError>> UpdateCurrentLatestSeason(
        SeasonUpdater seasonUpdater,
        SeasonStorageData data,
        CancellationToken cancellationToken) =>
        DemotableSeason(data).Bind(season => seasonUpdater(season, cancellationToken));

    private static Result<SeasonStorage, DomainError> DemotableSeason(SeasonStorageData data) => data switch
    {
        CurrentLatestSeason latest => DemoteCurrentLatestSeason(latest.Season),
        _ => Error.Create($"Season data is not selectable to be updated as is not the current lastest season. Received {data.GetType().Name}")
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