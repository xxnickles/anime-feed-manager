using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

internal static class TvStorageEnrichment
{
    internal static Task<Result<ScrapTvLibraryData>> AddDataFromStorage(
        this Task<Result<ScrapTvLibraryData>> data,
        StoredSeries storedSeries,
        TimeProvider timeProvider,
        CancellationToken token = default) =>
        data.Bind(d => AddExistentDataFromStorage(d, storedSeries, timeProvider, token));

    private static Task<Result<ScrapTvLibraryData>> AddExistentDataFromStorage(
        ScrapTvLibraryData scrapTvLibraryData,
        StoredSeries storedSeries,
        TimeProvider timeProvider,
        CancellationToken token = default)
    {
        var isOldSeason = Utils.IsOldSeason(scrapTvLibraryData.Season, timeProvider);

        return storedSeries(scrapTvLibraryData.Season, token)
            .Map(series => scrapTvLibraryData.SeriesData.Select(s =>
                ProcessSeriesData(s, scrapTvLibraryData.FeedData, series, isOldSeason)))
            .Map(seriesData => scrapTvLibraryData with { SeriesData = seriesData });
    }

    private static StorageData ProcessSeriesData(
        StorageData storageSeries,
        ImmutableList<FeedData> feedData,
        ImmutableList<TvSeriesInfo> existentSeries,
        bool isOldSeason)
    {
        var currentInfo = existentSeries.FirstOrDefault(s => s.Title == storageSeries.Series.Title);
        var feedDataInProcess = feedData.TryGetFeedMatch(storageSeries.Series.Title ?? string.Empty);

        var baseSeries = storageSeries.Series;

        if (currentInfo is not null)
        {
            return ProcessExistentSeries(
                storageSeries,
                baseSeries,
                currentInfo,
                feedDataInProcess,
                isOldSeason);
        }

        if (feedDataInProcess is not null)
        {
            baseSeries.Status = SeriesStatus.OngoingValue;
            baseSeries.FeedTitle = feedDataInProcess.Title;
            baseSeries.FeedLink = feedDataInProcess.Url;
            return storageSeries with { Series = baseSeries, Status = Status.NewSeries };
        }

        if (!isOldSeason) return storageSeries with { Status = Status.NewSeries };

        baseSeries.Status = SeriesStatus.Completed();
        return storageSeries with { Series = baseSeries, Status = Status.NewSeries };
    }

    private static StorageData ProcessExistentSeries(
        StorageData storageSeries,
        AnimeInfoStorage baseSeries,
        TvSeriesInfo currentInfo,
        FeedData? feedDataInProcess,
        bool isOldSeason)
    {
        if (!string.IsNullOrWhiteSpace(currentInfo.FeedTitle) || !string.IsNullOrWhiteSpace(currentInfo.FeedUrl))
        {
            baseSeries.FeedTitle = currentInfo.FeedTitle;
            baseSeries.FeedLink = currentInfo.FeedUrl;
        }

        var needToUpdateFeedTitle = string.IsNullOrWhiteSpace(currentInfo.FeedTitle) && feedDataInProcess is not null;
        if (needToUpdateFeedTitle)
        {
            baseSeries.FeedTitle = feedDataInProcess?.Title;
            baseSeries.FeedLink = feedDataInProcess?.Url;
        }

        baseSeries.Status = CalculateSeriesStatus(currentInfo.Status, feedDataInProcess is not null, isOldSeason);
        baseSeries.AlternativeTitles = currentInfo.AlternativeTitles.AppArrayToString();

        if (currentInfo is not TvSeriesInfoWithImage withImage)
            return storageSeries with
            {
                Series = baseSeries,
                Status = needToUpdateFeedTitle || baseSeries.Status != currentInfo.Status
                    ? Status.UpdatedSeries
                    : Status.NoChanges
            };

        baseSeries.ImagePath = withImage.ImageUrl;
        return new StorageData(baseSeries, new AlreadyExistInSystem(), Status.UpdatedSeries);
    }

    private static string CalculateSeriesStatus(SeriesStatus currentStatus, bool hasFeedMatch, bool isOldSeason) =>
        (currentStatus.ToString(), hasFeedMatch) switch
        {
            (SeriesStatus.NotAvailableValue, true) => SeriesStatus.OngoingValue,
            (SeriesStatus.NotAvailableValue, false) => isOldSeason
                ? SeriesStatus.Completed()
                : SeriesStatus.NotAvailable(),
            (SeriesStatus.OngoingValue, false) => SeriesStatus.Completed(),
            (SeriesStatus.OngoingValue, true) => SeriesStatus.Ongoing(),
            (_, _) => SeriesStatus.NotAvailable(),
        };
}
