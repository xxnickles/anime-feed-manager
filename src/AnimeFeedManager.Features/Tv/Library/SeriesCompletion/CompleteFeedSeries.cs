using AnimeFeedManager.Features.Tv.FeedManager.Events;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.SeriesCompletion;

[Widens]
public partial union SeriesCompletionError(UnreadableMessage, StorageError);

public static class CompleteFeedSeries
{
    /// <summary>Marks one series Completed; a series already Completed is not written, so redelivery is harmless.</summary>
    public static Task<Result<Unit, SeriesCompletionError>> Complete(
        string seriesId,
        TvSeriesGetter seriesGetter,
        TvSeriesStorageUpdater storageUpdater,
        CancellationToken token) =>
        IdHelpers.SeriesSeasonFromId(seriesId)
            .MapError(SeriesCompletionError (fields) => new UnreadableMessage(typeof(NyaaFeedCompleted), fields.Errors))
            .Bind(season => OptimisticConcurrency.UntilWritten(
                attemptToken => seriesGetter(seriesId, season, attemptToken)
                    .Bind(series => series.Status == SeriesStatus.CompletedValue
                        ? Task.FromResult(Result<Unit, StorageError>.Success(new Unit()))
                        : storageUpdater(MarkCompleted(series), attemptToken)),
                token,
                error => error is StorageError.Conflict))
            .WithOperationName(nameof(CompleteFeedSeries))
            .WithLogProperty(Telemetry.SeriesIdTag, seriesId);

    private static AnimeInfoStorage MarkCompleted(AnimeInfoStorage series)
    {
        series.Status = SeriesStatus.Completed();
        return series;
    }
}
