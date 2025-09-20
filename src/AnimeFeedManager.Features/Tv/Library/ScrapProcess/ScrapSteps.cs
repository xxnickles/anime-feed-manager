using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.Types;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class ScrapSteps
{
    public static Task<Result<ScrapTvLibraryData>> ScrapSeries(this Task<Result<ImmutableList<string>>> feedTitles,
        SeasonSelector season, PuppeteerOptions puppeteerOptions) =>
        feedTitles.Bind(titles => GetInitialProcessData(titles, season, puppeteerOptions));

    public static Task<Result<ScrapTvLibraryData>> AddDataFromStorage(this Task<Result<ScrapTvLibraryData>> data,
        StoredSeries storedSeries,
        TimeProvider timeProvider,
        CancellationToken token = default) =>
        data.Bind(d => AddExistentDataFromStorage(d, storedSeries, timeProvider, token));

    private static async Task<Result<ScrapTvLibraryData>> GetInitialProcessData(
        ImmutableList<string> titles,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions)
    {
        // Scrapping
        try
        {
            var (series, jsonSeason) =
                await AniDbScrapper.Scrap(Utils.CreateScrappingLink(season), puppeteerOptions);

            return (jsonSeason.Season, jsonSeason.Year, season is Latest)
                .ParseAsSeriesSeason()
                .Map(seriesSeason => new ScrapTvLibraryData(
                    series.Select(Transform),
                    titles,
                    seriesSeason));

            // To keep it simple, we are using the season information coming from the scrapping instead of the parsed one
            // But at this point we are sure Season is valid as we have parsed the data scrapped 
            StorageData Transform(SeriesContainer seriesContainer)
            {
                return new StorageData(
                    new AnimeInfoStorage
                    {
                        RowKey = seriesContainer.Id,
                        PartitionKey = IdHelpers.GenerateAnimePartitionKey(jsonSeason.Season, (ushort) jsonSeason.Year),
                        Title = seriesContainer.Title,
                        Synopsis = seriesContainer.Synopsys,
                        FeedTitle = string.Empty,
                        Date = SharedUtils.ParseDate(seriesContainer.Date, seriesContainer.SeasonInfo.Year)
                            ?.ToUniversalTime(),
                        Status = SeriesStatus.NotAvailableValue
                    },
                    Uri.TryCreate(seriesContainer.ImageUrl, UriKind.Absolute, out var validUri)
                        ? new ScrappedImageUrl(validUri)
                        : new NoImage());
            }
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static Task<Result<ScrapTvLibraryData>> AddExistentDataFromStorage(
        ScrapTvLibraryData scrapTvLibraryData,
        StoredSeries storedSeries,
        TimeProvider timeProvider,
        CancellationToken token = default)
    {
        var isOldSeason = Utils.IsOldSeason(scrapTvLibraryData.Season, timeProvider);

        return storedSeries(scrapTvLibraryData.Season, token)
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
                (SeriesStatus.NotAvailableValue, false) => isOldSeason
                    ? SeriesStatus.Completed()
                    : SeriesStatus.NotAvailable(),
                (SeriesStatus.OngoingValue, false) => SeriesStatus.Completed(),
                (SeriesStatus.OngoingValue, true) => SeriesStatus.Ongoing(),
                (_, _) => SeriesStatus.NotAvailable(),
            };
            baseSeries.AlternativeTitles = currentInfo.AlternativeTitles.ArrayToString();

            if (currentInfo is not TvSeriesInfoWithImage withImage)
                return storageSeries with {Series = baseSeries};

            baseSeries.ImagePath = withImage.ImageUrl;
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

        baseSeries.Status = SeriesStatus.Completed();
        return storageSeries with {Series = baseSeries};
    }
}