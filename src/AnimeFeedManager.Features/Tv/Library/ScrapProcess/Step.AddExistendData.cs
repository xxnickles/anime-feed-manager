namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class AddExistentDataStep
{
    internal static Task<Result<ScrapTvLibraryData>> AddLocalDataToScrappedSeries(
        this Task<Result<ScrapTvLibraryData>> processResult,
        StoredSeriesGetter storedSeriesGetter,
        TimeProvider timeProvider,
        CancellationToken token = default) =>
        processResult.Bind(process => AddExistentDataFromStorage(storedSeriesGetter, process, timeProvider, token));
   
    internal static Task<Result<ScrapTvLibraryData>> AddExistentDataFromStorage(
        StoredSeriesGetter storedSeriesGetter,
        ScrapTvLibraryData scrapTvLibraryData,
        TimeProvider timeProvider,
        CancellationToken token = default)
    {
        var isOldSeason = UpdateLibraryUtils.IsOldSeason(scrapTvLibraryData.Season, timeProvider);

        return storedSeriesGetter(scrapTvLibraryData.Season, token)
            .Map(series => scrapTvLibraryData.SeriesData.Select(s =>
                ProcessSeriesData(s, scrapTvLibraryData.FeedTitles, series, isOldSeason)))
            .Map(seriesData => scrapTvLibraryData with {SeriesData = seriesData});
    }


    private static StorageData ProcessSeriesData(
        StorageData storageSeries,
        ImmutableList<string> feedTitles,
        ImmutableList<TvSeriesInfo> existentSeries,
        bool isOldSeason)
    {
        var currentInfo = existentSeries.FirstOrDefault(s => s.Title == storageSeries.Series.Title);
        var feedTitleInProcess = feedTitles.TryGetFeedTitle(storageSeries.Series.Title ?? string.Empty);
        var processHasFeedTitle = !string.IsNullOrEmpty(feedTitleInProcess);
        var baseSeries = storageSeries.Series;
        // Series already exist
        if (currentInfo is not null)
        {
            baseSeries.FeedTitle = currentInfo.FeedTitle;
            baseSeries.Status = (currentInfo.Status.ToString(), processHasFeedTitle) switch
            {
                (SeriesStatus.NotAvailableValue, true) => SeriesStatus.OngoingValue,
                (SeriesStatus.OngoingValue, false) => SeriesStatus.Completed,
                (SeriesStatus.OngoingValue, true) => SeriesStatus.Ongoing,
                (_, _) => SeriesStatus.NotAvailable,
            };
            baseSeries.AlternativeTitles = currentInfo.AlternativeTitles.ArrayToString();
            if (currentInfo is not TvSeriesInfoWithImage withImage)
                return storageSeries with {Series = baseSeries};

            baseSeries.ImageUrl = withImage.ImageUrl;
            return new StorageData(baseSeries, new AlreadyExistInSystem());
        }
        // New series, there is a matching feed

        if (processHasFeedTitle)
        {
            baseSeries.Status = SeriesStatus.OngoingValue;
            baseSeries.FeedTitle = feedTitleInProcess;
            return storageSeries with {Series = baseSeries};
        }

        if (!isOldSeason) return storageSeries;

        baseSeries.Status = SeriesStatus.Completed;
        return storageSeries with {Series = baseSeries};
    }
}