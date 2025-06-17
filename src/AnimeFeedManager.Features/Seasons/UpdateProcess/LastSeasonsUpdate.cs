namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public static class LastSeasonsUpdate
{
    private static Task<Result<IEnumerable<SeriesSeason>>> GetLast4Season(AllSeasonsGetter seasonsGetter,
        CancellationToken cancellationToken) =>
        seasonsGetter(cancellationToken)
            .Map(seasons => seasons
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Season)
                .Take(4)
                .Reverse());

    private static Task<Result<Unit>> StoreLatestSeason(
        LastestSeasonsUpdater latestSeasonUpdater,
        IEnumerable<SeriesSeason> seasons,
        CancellationToken cancellationToken)
    {
        return latestSeasonUpdater(new LatestSeasonsStorage
        {
            Payload = JsonSerializer.Serialize(seasons, SeriesSeasonContext.Default.SeriesSeasonArray)
        }, cancellationToken);
    }

    public static Task<Result<SeasonUpdateData>> UpdateLast4Seasons(
        this Task<Result<SeasonUpdateData>> result,
        AllSeasonsGetter seasonsGetter,
        LastestSeasonsUpdater latestSeasonUpdater,
        CancellationToken cancellationToken) =>
        result.Bind(r => r.SeasonData switch
        {
            NoUpdateRequired => Task.FromResult(Result<SeasonUpdateData>.Success(r)),
            _ => GetLast4Season(seasonsGetter, cancellationToken)
                .Bind(s => StoreLatestSeason(latestSeasonUpdater, s, cancellationToken))
                .Map(_ => r)
        });
}