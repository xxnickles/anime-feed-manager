using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

internal static class TvStorageEnrichment
{
    internal static Task<Result<ScrapTvLibraryData>> AddDataFromStorage(
        this Task<Result<ScrapTvLibraryData>> data,
        StoredSeries storedSeries,
        CancellationToken token = default) =>
        data.Bind(d => AddExistentDataFromStorage(d, storedSeries, token));

    private static Task<Result<ScrapTvLibraryData>> AddExistentDataFromStorage(
        ScrapTvLibraryData scrapTvLibraryData,
        StoredSeries storedSeries,
        CancellationToken token = default)
    {
        return storedSeries(scrapTvLibraryData.Season, token)
            .Map(series => scrapTvLibraryData.SeriesData.Select(s =>
                ProcessSeriesData(s, scrapTvLibraryData.FeedData, series)))
            .Map(seriesData => scrapTvLibraryData with { SeriesData = seriesData });
    }

    private static StorageData ProcessSeriesData(
        StorageData storageSeries,
        ImmutableArray<FeedData> feedData,
        ImmutableArray<TvSeriesInfo> existentSeries)
    {
        var currentInfo = existentSeries.FirstOrDefault(s => s.Title == storageSeries.Series.Title);
        var feedDataInProcess = feedData.TryGetFeedMatch(storageSeries.Series.Title ?? string.Empty);

        var baseSeries = storageSeries.Series;

        if (currentInfo is not null)
        {
            return ProcessExistentSeries(storageSeries, baseSeries, currentInfo, feedDataInProcess);
        }

        if (feedDataInProcess is not null)
        {
            baseSeries.FeedTitle = feedDataInProcess.Title;
            baseSeries.FeedLink = feedDataInProcess.Url;
        }

        // Nothing stored, so no transition to guard against.
        baseSeries.Status = ResolveStatus(storageSeries.Airing, feedDataInProcess is not null, null);
        return storageSeries with { Series = baseSeries, Status = Status.NewSeries };
    }

    private static StorageData ProcessExistentSeries(
        StorageData storageSeries,
        AnimeInfoStorage baseSeries,
        TvSeriesInfo currentInfo,
        FeedData? feedDataInProcess)
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

        baseSeries.Status = ResolveStatus(storageSeries.Airing, feedDataInProcess is not null, currentInfo.Status);

        // This scrap owns the provider block; the stored user block survives it untouched.
        baseSeries.AlternativeTitles = (StoredAlternativeTitles.Parse(baseSeries.AlternativeTitles) with
        {
            User = currentInfo.AlternativeTitles.User
        }).ToStoredString();

        if (currentInfo is not TvSeriesInfoWithImage withImage)
            return storageSeries with
            {
                Series = baseSeries,
                Status = needToUpdateFeedTitle || baseSeries.Status != currentInfo.Status
                    ? Status.UpdatedSeries
                    : Status.NoChanges
            };

        baseSeries.ImagePath = withImage.ImageUrl;
        return new StorageData(baseSeries, new AlreadyExistInSystem(), Status.UpdatedSeries, storageSeries.Airing);
    }

    /// <summary>
    /// The feed decides availability; the provider only resolves what "not in the feed" means.
    /// <paramref name="stored"/> is consulted solely to veto a forbidden transition — a series that
    /// has aired never rewinds to NotAvailable — never to derive the new status.
    /// </summary>
    private static string ResolveStatus(AiringStatus airing, bool hasFeedMatch, SeriesStatus? stored)
    {
        if (hasFeedMatch) return SeriesStatus.OngoingValue;
        if (airing is AiringStatus.Finished) return SeriesStatus.CompletedValue;

        // Held at its stored value, the completion sweep finishes the job on feed absence.
        var current = stored?.ToString();
        return current is SeriesStatus.OngoingValue or SeriesStatus.CompletedValue
            ? current
            : SeriesStatus.NotAvailableValue;
    }
}
