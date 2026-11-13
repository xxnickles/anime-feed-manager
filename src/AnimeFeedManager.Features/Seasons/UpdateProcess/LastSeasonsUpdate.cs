using CommonJsonContext = AnimeFeedManager.Features.Common.CommonJsonContext;

namespace AnimeFeedManager.Features.Seasons.UpdateProcess;

public static class LastSeasonsUpdate
{
    public static Task<Result<SeasonUpdateData, DomainError>> UpdateLast4Seasons(
        this Task<Result<SeasonUpdateData, DomainError>> result,
        AllSeasonsGetter seasonsGetter,
        LastestSeasonsUpdater latestSeasonUpdater,
        CancellationToken cancellationToken) =>
        result.Bind(r => r.SeasonData switch
        {
            NoUpdateRequired => Task.FromResult(Result<SeasonUpdateData, StorageError>.Success(r)),
            _ => GetLast4Season(seasonsGetter, cancellationToken)
                .Bind(s => StoreLatestSeason(latestSeasonUpdater, s, cancellationToken))
                .Map(_ => r)
        });

    private static Task<Result<IEnumerable<SeriesSeason>, StorageError>> GetLast4Season(AllSeasonsGetter seasonsGetter,
        CancellationToken cancellationToken) =>
        seasonsGetter(cancellationToken)
            .Map(seasons => seasons
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Season)
                .Take(4)
                .Reverse());

    private static Task<Result<Unit, StorageError>> StoreLatestSeason(
        LastestSeasonsUpdater latestSeasonUpdater,
        IEnumerable<SeriesSeason> seasons,
        CancellationToken cancellationToken)
    {
        return latestSeasonUpdater(new LatestSeasonsStorage
        {
            Payload = JsonSerializer.Serialize(seasons.ToArray(), CommonJsonContext.Default.SeriesSeasonArray)
        }, cancellationToken);
    }
}