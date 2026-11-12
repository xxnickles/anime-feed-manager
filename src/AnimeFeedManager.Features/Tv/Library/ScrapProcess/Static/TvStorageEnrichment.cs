using AnimeFeedManager.Features.Common.Scrapping;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

internal static class TvStorageEnrichment
{
    internal static Task<Result<ScrapTvLibraryData, DomainError>> AddDataFromStorage(
        this Task<Result<ScrapTvLibraryData, DomainError>> data,
        StoredSeriesGetter storedSeriesGetter,
        CancellationToken token = default) =>
        data.Bind(d => AddExistentDataFromStorage(d, storedSeriesGetter, token));

    private static Task<Result<ScrapTvLibraryData, DomainError>> AddExistentDataFromStorage(
        ScrapTvLibraryData scrapTvLibraryData,
        StoredSeriesGetter storedSeriesGetter,
        CancellationToken token = default)
    {
        return storedSeriesGetter(scrapTvLibraryData.Season, token)
            .Map(series => scrapTvLibraryData.SeriesData.Select(s => ProcessSeriesData(s, series)))
            .Map(seriesData => scrapTvLibraryData with { SeriesData = seriesData });
    }

    // Feed fields stay null: the library write merges, so a stored binding's feed survives import.
    private static StorageData ProcessSeriesData(StorageData storageSeries, ImmutableArray<TvSeriesInfo> existentSeries)
    {
        var currentInfo = existentSeries.FirstOrDefault(s => s.Title == storageSeries.Series.Title);
        var baseSeries = storageSeries.Series;

        if (currentInfo is not null)
            return ProcessExistentSeries(storageSeries, baseSeries, currentInfo);

        baseSeries.Status = ResolveStatus(storageSeries.Airing, null);
        return storageSeries with { Series = baseSeries, Status = Status.NewSeries };
    }

    private static StorageData ProcessExistentSeries(
        StorageData storageSeries,
        AnimeInfoStorage baseSeries,
        TvSeriesInfo currentInfo)
    {
        baseSeries.Status = ResolveStatus(storageSeries.Airing, currentInfo.Status);

        if (currentInfo is not TvSeriesInfoWithImage withImage)
            return storageSeries with
            {
                Series = baseSeries,
                Status = baseSeries.Status != currentInfo.Status ? Status.UpdatedSeries : Status.NoChanges
            };

        baseSeries.ImagePath = withImage.ImageUrl;
        return new StorageData(baseSeries, new AlreadyExistInSystem(), Status.UpdatedSeries, storageSeries.Airing);
    }

    /// <summary>
    /// A binding makes a series Ongoing and only feed completion ends it, so a stored Ongoing outranks the
    /// provider; otherwise Finished completes, a stored Completed holds, and the rest is NotAvailable.
    /// </summary>
    private static string ResolveStatus(AiringStatus airing, SeriesStatus? stored)
    {
        var current = stored?.ToString();
        if (current is SeriesStatus.OngoingValue) return current;
        if (airing is AiringStatus.Finished) return SeriesStatus.CompletedValue;
        return current is SeriesStatus.CompletedValue ? current : SeriesStatus.NotAvailableValue;
    }
}
