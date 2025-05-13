using AnimeFeedManager.Features.Scrapping.AniDb;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tv.Library.UpdateProcess;

internal record UpdateLibraryProcessData(
    IEnumerable<AnimeInfoStorage> Series,
    ImmutableList<TvSeriesInfo> ExistentSeries,
    ImmutableList<string> FeedTitles,
    SeriesSeason Season);

internal delegate Task<Result<ImmutableList<string>>> FeedTitlesProvider();
    
internal static class UpdateLibrarySteps
{
    #region Convinience Composition Methods

    internal static Task<Result<UpdateLibraryProcessData>> StartProcess(
        this Task<Result<ImmutableList<string>>> titlesProvider,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions) =>
        titlesProvider
            .Bind(titles => GetProcessData(titles, season, puppeteerOptions));

    internal static Task<Result<UpdateLibraryProcessData>> AddStorageSeriesData(
        this Task<Result<UpdateLibraryProcessData>> processResult,
        StoredSeriesGetter storedSeriesGetter,
        CancellationToken token) =>
        processResult.Bind(process => AddExistentSeries(storedSeriesGetter, process, token));

    internal static Task<Result<UpdateLibraryProcessData>> AddLocalDataToScrappedSeries(
        this Task<Result<UpdateLibraryProcessData>> processResult,
        TimeProvider timeProvider) =>
        processResult.Map(process => UpdateSeriesData(process, timeProvider));

    #endregion


    private static async Task<Result<UpdateLibraryProcessData>> GetProcessData(
        ImmutableList<string> titles,
        SeasonSelector season,
        PuppeteerOptions puppeteerOptions,
        CancellationToken token = default)
    {
        // Scrapping
        var (series, jsonSeason) =
            await AniDbScrapper.Scrap(UpdateLibraryUtils.CreateScrappingLink(season), puppeteerOptions);

        return (jsonSeason.Season, jsonSeason.Year, season is Latest)
            .ParseAsSeriesSeason()
            .Map(seriesSeason => new UpdateLibraryProcessData(
                series.Select(Transform),
                [],
                titles,
                seriesSeason));

        // To keep it simple, we are using the season information coming from the scrapping instead of the parsed one
        // But at this point we are sure Season is valid as we have parsed the data scrapped 
        AnimeInfoStorage Transform(SeriesContainer seriesContainer)
        {
            return new AnimeInfoStorage
            {
                RowKey = seriesContainer.Id,
                PartitionKey = IdHelpers.GenerateAnimePartitionKey(jsonSeason.Season, (ushort) jsonSeason.Year),
                Title = seriesContainer.Title,
                Synopsis = seriesContainer.Synopsys,
                FeedTitle = string.Empty,
                Date = Utils.ParseDate(seriesContainer.Date, seriesContainer.SeasonInfo.Year)?.ToUniversalTime(),
                Status = SeriesStatus.NotAvailableValue,
                Season = seriesContainer.SeasonInfo.Season,
                Year = seriesContainer.SeasonInfo.Year
            };
        }
    }

    private static Task<Result<UpdateLibraryProcessData>> AddExistentSeries(
        StoredSeriesGetter storedSeriesGetter,
        UpdateLibraryProcessData updateLibraryProcessData,
        CancellationToken token)
    {
        return storedSeriesGetter(updateLibraryProcessData.Season, token)
            .Map(series => updateLibraryProcessData with {ExistentSeries = series});
    }

    internal static UpdateLibraryProcessData UpdateSeriesData(UpdateLibraryProcessData updateLibraryProcessData,
        TimeProvider timeProvider)
    {
        var isOldSeason = UpdateLibraryUtils.IsOldSeason(updateLibraryProcessData.Season, timeProvider);
        var updatedSeries =
            updateLibraryProcessData.Series.Select(s => ProcessSeriesData(s, updateLibraryProcessData, isOldSeason));
        return updateLibraryProcessData with {Series = updatedSeries};
    }


    private static AnimeInfoStorage ProcessSeriesData(AnimeInfoStorage baseSeries,
        UpdateLibraryProcessData updateLibraryProcessData, bool isOldSeason)
    {
        var currentInfo = updateLibraryProcessData.ExistentSeries.FirstOrDefault(s => s.Title == baseSeries.Title);
        var feedTitleInProcess = updateLibraryProcessData.FeedTitles.TryGetFeedTitle(baseSeries.Title ?? string.Empty);
        var processHasFeedTitle = !string.IsNullOrEmpty(feedTitleInProcess);
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
        }
        // New series, there is a matching feed
        else if (processHasFeedTitle)
        {
            baseSeries.Status = SeriesStatus.OngoingValue;
            baseSeries.FeedTitle = feedTitleInProcess;
        }
        else if (isOldSeason)
        {
            baseSeries.Status = SeriesStatus.Completed;
        }

        return baseSeries;
    }
}