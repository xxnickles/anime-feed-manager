using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

public static class CompleteFeedSeries
{
    /// <summary>Marks one series Completed; a series already Completed is not written, so redelivery is harmless.</summary>
    public static Task<Result<Unit>> Complete(
        string seriesId,
        TvSeriesGetter seriesGetter,
        TvSeriesStorageUpdater storageUpdater,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(seriesId)
            .Bind(season => OptimisticConcurrency.UntilWritten(
                attemptToken => seriesGetter(seriesId, season, attemptToken)
                    .Bind(series => series.Status == SeriesStatus.CompletedValue
                        ? Task.FromResult(Result<Unit>.Success())
                        : storageUpdater(MarkCompleted(series), attemptToken)),
                token,
                error => error is PreconditionFailedError))
            .WithOperationName(nameof(CompleteFeedSeries))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    private static AnimeInfoStorage MarkCompleted(AnimeInfoStorage series)
    {
        series.Status = SeriesStatus.Completed();
        return series;
    }
}
