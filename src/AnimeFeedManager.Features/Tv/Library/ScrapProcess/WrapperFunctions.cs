using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class Wrappers
{
    public static Task<Result<ScrapTvLibraryData>> ScrapTvSeries(
        this Result<SeasonSelector> seasonSelector,
        ITvLibraryScrapper scrapper,
        CancellationToken token) => seasonSelector.Bind(season => scrapper.ScrapTvSeries(season, token));
    
    public static Task<Result<ScrapTvLibraryData>> AddImagesLinks(
        this Task<Result<ScrapTvLibraryData>> processData,
        ITvImagesCollector imagesCollector,
        CancellationToken token) =>
        processData.Bind(data => imagesCollector.AddImagesLink(data, token).Map(_ => data));

    public static Task<Result<ScrapTvLibraryData>> UpdateTvLibrary(
        this Task<Result<ScrapTvLibraryData>> processData,
        TvLibraryStorageUpdater libraryStorageUpdater,
        CancellationToken token) => processData
        .Bind(process => libraryStorageUpdater(process.SeriesData.Select(s => s.Series), token)
            .Map(_ => process));


    public static Task<Result<ScrapTvLibraryResult>> SendEvents(
        this Task<Result<ScrapTvLibraryData>> processData,
        IDomainPostman domainPostman,
        CancellationToken token) => processData
        .Bind(data => domainPostman.SendMessages(GetEvents(data), token)
            .Map(_ => ExtractResults(data))
        );
        

    private static DomainMessage[] GetEvents(ScrapTvLibraryData data) =>
    [
        new SeasonUpdated(data.Season),
        new NewSeriesAdded(data.SeriesData.Where(d => d.Image is AlreadyExistInSystem)
            .Select(d => d.Series.Title ?? string.Empty).ToArray()),
        new FeedUpdated(data.FeedTitles.ToArray())
    ];

    private static ScrapTvLibraryResult ExtractResults(ScrapTvLibraryData data) =>
        new (data.Season,
            data.SeriesData.Count(d => d.Image is AlreadyExistInSystem),
            data.SeriesData.Count(d => d.Image is not AlreadyExistInSystem));
}