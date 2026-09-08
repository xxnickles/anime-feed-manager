namespace AnimeFeedManager.Features.Tv.Library.Management;

public static class Series
{
    public static Task<Result<Unit>> UpdateAlternativeTitles(
        string seriesId,
        string seriesSeason,
        string[] alternativeTitles,
        TvSeriesGetter seriesGetter,
        TvSeriesStorageUpdater storageUpdater,
        CancellationToken token = default) => seriesGetter(seriesId, seriesSeason, token)
        .Map(series => UpdateAlternativeTitles(series, alternativeTitles))
        .Bind(series => storageUpdater(series, token));


    public static Task<Result<Unit>> DeleteSeries(
        string seriesId, 
        string seriesSeason,
        TvSeriesRemover seriesRemover,
        CancellationToken token) => seriesRemover(seriesId, seriesSeason, token);


    // The editor owns the user block only; the provider block is left as the last scrap wrote it.
    private static AnimeInfoStorage UpdateAlternativeTitles(
        AnimeInfoStorage storage,
        string[] alternativeTitles)
    {
        storage.AlternativeTitles = (StoredAlternativeTitles.Parse(storage.AlternativeTitles) with
        {
            User = alternativeTitles
        }).ToStoredString();
        return storage;
    }
}