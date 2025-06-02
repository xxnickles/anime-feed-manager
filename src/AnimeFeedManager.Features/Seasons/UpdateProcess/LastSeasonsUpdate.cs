using System.Text.Json;

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

    public static Task<Result<T>> UpdateLast4Season<T>(
        this Task<Result<T>> result,
        AllSeasonsGetter seasonsGetter,
        LastestSeasonsUpdater latestSeasonUpdater,
        Func<T,bool> shouldUpdateWhen,
        CancellationToken cancellationToken) =>
        result.Bind(r => shouldUpdateWhen(r) switch
        {
            true => GetLast4Season(seasonsGetter, cancellationToken)
                .Bind(s => StoreLatestSeason(latestSeasonUpdater, s, cancellationToken))
                .Map(_ => r),
            false => Task.FromResult(Result<T>.Success(r))
        });
}