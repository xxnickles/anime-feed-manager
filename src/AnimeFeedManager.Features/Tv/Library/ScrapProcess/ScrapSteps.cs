using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.Types;
using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class ScrapSteps
{
    public static Task<Result<ScrapTvLibraryData>> ScrapSeries(
        this Task<Result<ImmutableList<FeedData>>> feedTitles,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions) =>
        feedTitles.Bind(titles => GetInitialProcessData(titles, season, puppeteerOptions));

    public static Task<Result<ScrapTvLibraryData>> AddDataFromStorage(this Task<Result<ScrapTvLibraryData>> data,
        StoredSeries storedSeries,
        TimeProvider timeProvider,
        CancellationToken token = default) =>
        data.Bind(d => AddExistentDataFromStorage(d, storedSeries, timeProvider, token));

    private static async Task<Result<ScrapTvLibraryData>> GetInitialProcessData(
        ImmutableList<FeedData> feedData,
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
                .WithOperationName(nameof(GetInitialProcessData))
                .AddLogOnSuccess((seriesSeason => logger =>
                    logger.LogInformation("{Count} series has been scrapped for season {Season}", series.Count(),
                        seriesSeason)))
                .WithLogProperty("Season", season)
                .Map(seriesSeason => new ScrapTvLibraryData(
                    series.Select(Transform),
                    feedData,
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
                        FeedTitle = null,
                        FeedLink = null,
                        Date = SharedUtils.ParseDate(seriesContainer.Date, seriesContainer.SeasonInfo.Year)
                            ?.ToUniversalTime(),
                        Status = SeriesStatus.NotAvailableValue
                    },
                    Uri.TryCreate(seriesContainer.ImageUrl, UriKind.Absolute, out var validUri)
                        ? new ScrappedImageUrl(validUri)
                        : new NoImage(),
                    Status.NewSeries);
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
                ProcessSeriesData(s, scrapTvLibraryData.FeedData, series, isOldSeason)))
            .Map(seriesData => scrapTvLibraryData with {SeriesData = seriesData});
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
        // Series already exist
        if (currentInfo is not null)
        {
            return ProcessExistentSeries(
                storageSeries,
                baseSeries,
                currentInfo,
                feedDataInProcess,
                isOldSeason);
        }

        // New series, there is a matching feed
        if (feedDataInProcess is not null)
        {
            baseSeries.Status = SeriesStatus.OngoingValue;
            baseSeries.FeedTitle = feedDataInProcess.Title;
            baseSeries.FeedLink = feedDataInProcess.Url;
            return storageSeries with {Series = baseSeries, Status = Status.NewSeries};
        }

        if (!isOldSeason) return storageSeries with {Status = Status.NewSeries};

        baseSeries.Status = SeriesStatus.Completed();
        return storageSeries with {Series = baseSeries, Status = Status.NewSeries};
    }

    private static StorageData ProcessExistentSeries(
        StorageData storageSeries,
        AnimeInfoStorage baseSeries,
        TvSeriesInfo currentInfo,
        FeedData? feedDataInProcess,
        bool isOldSeason)
    {
        // Copy existing feed info from storage if it exists. Checks both feed title and feed url as one of them can be empty
        if (!string.IsNullOrWhiteSpace(currentInfo.FeedTitle) || !string.IsNullOrWhiteSpace(currentInfo.FeedUrl))
        {
            baseSeries.FeedTitle = currentInfo.FeedTitle;
            baseSeries.FeedLink = currentInfo.FeedUrl;
        }

        // Updates feed info if needed (override with new feed data if current is empty and new data available)
        var needToUpdateFeedTitle = string.IsNullOrWhiteSpace(currentInfo.FeedTitle) && feedDataInProcess is not null;
        if (needToUpdateFeedTitle)
        {
            baseSeries.FeedTitle = feedDataInProcess?.Title;
            baseSeries.FeedLink = feedDataInProcess?.Url;
        }


        baseSeries.Status = (currentInfo.Status.ToString(), feedDataInProcess is not null) switch
        {
            (SeriesStatus.NotAvailableValue, true) => SeriesStatus.OngoingValue,
            (SeriesStatus.NotAvailableValue, false) => isOldSeason
                ? SeriesStatus.Completed()
                : SeriesStatus.NotAvailable(),
            (SeriesStatus.OngoingValue, false) => SeriesStatus.Completed(),
            (SeriesStatus.OngoingValue, true) => SeriesStatus.Ongoing(),
            (_, _) => SeriesStatus.NotAvailable(),
        };
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
}