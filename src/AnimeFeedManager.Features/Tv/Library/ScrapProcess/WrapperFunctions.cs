using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class Wrappers
{
    public static Task<Result<ScrapTvLibraryData>> ScrapTvSeries(
        this Result<SeasonSelector> seasonSelector,
        ITvLibraryScrapper scrapper,
        CancellationToken token) => seasonSelector.Bind(season => scrapper.ScrapTvSeries(season, token));
    
    public static Task<Result<ScrapTvLibraryData>> AddImagesLinks(
        this Task<Result<ScrapTvLibraryData>> processData,
        IImageProvider imagesProvider,
        ILogger logger,
        CancellationToken token) =>
        processData.Bind(data => imagesProvider.AddImagesLink(data, logger, token));

    public static Task<Result<ScrapTvLibraryData>> UpdateTvLibrary(
        this Task<Result<ScrapTvLibraryData>> processData,
        TvLibraryStorageUpdater libraryStorageUpdater,
        CancellationToken token) => processData
        .Bind(process => libraryStorageUpdater(process.SeriesData.Select(s => s.Series), token)
            .Map(_ => process));
}