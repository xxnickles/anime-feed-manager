namespace AnimeFeedManager.Features.Tv.Library.Management;

public static class AlternativeTitles
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


    private static AnimeInfoStorage UpdateAlternativeTitles(
        AnimeInfoStorage storage,
        string[] alternativeTitles)
    {
        storage.AlternativeTitles = alternativeTitles.Length > 0 ? alternativeTitles.ArrayToString() : string.Empty;
        return storage;
    }
}