namespace AnimeFeedManager.Features.Tv.Library.Management;

public static class Series
{
    /// <summary>Sets the episode count until the next import that carries one from the provider.</summary>
    public static Task<Result<Unit, StorageError>> UpdateExpectedEpisodes(
        string seriesId,
        SeriesSeason seriesSeason,
        int episodes,
        TvSeriesGetter seriesGetter,
        TvSeriesStorageUpdater storageUpdater,
        CancellationToken token = default) => OptimisticConcurrency.UntilWritten(
        attemptToken => seriesGetter(seriesId, seriesSeason, attemptToken)
            .Map(series =>
            {
                series.Episodes = episodes;
                return series;
            })
            .Bind(series => storageUpdater(series, attemptToken)),
        token,
        error => error is StorageError.Conflict);


    public static Task<Result<Unit, StorageError>> DeleteSeries(
        string seriesId,
        SeriesSeason seriesSeason,
        TvSeriesRemover seriesRemover,
        CancellationToken token) => seriesRemover(seriesId, seriesSeason, token);
}