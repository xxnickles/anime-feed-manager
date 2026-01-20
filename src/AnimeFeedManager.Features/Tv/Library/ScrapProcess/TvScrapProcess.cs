using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class TvScrapProcess
{
    public static Task<Result<ScrapTvLibraryData>> ScrapTvSeries(
        SeasonParameters? season,
        TvScrapper scrapper,
        ImageProcessor imagesProvider,
        TvLibraryStorageUpdater libraryStorageUpdater,
        CancellationToken token)
    {
        return TryGetSeasonSelector(season)
            .GetTvSeries(scrapper, token)
            .AddImagesLinks(imagesProvider, token)
            .UpdateTvLibrary(libraryStorageUpdater, token);
    }


    private static Task<Result<ScrapTvLibraryData>> GetTvSeries(
        this Result<SeasonSelector> seasonSelector,
        TvScrapper scrapper,
        CancellationToken token) => seasonSelector.Bind(season => scrapper(season, token));
    
    private static Result<SeasonSelector> TryGetSeasonSelector(SeasonParameters? season)
    {
        if (season is null)
            return new Latest();

        return (season.Season, season.Year, false)
            .ParseAsSeriesSeason()
            .Map<SeriesSeason, SeasonSelector>(parsedSeason => new BySeason(parsedSeason.Season, parsedSeason.Year));
    }
    
    // Just combinators to make the code more readable
    extension(Task<Result<ScrapTvLibraryData>> processData)
    {
        private Task<Result<ScrapTvLibraryData>> AddImagesLinks(ImageProcessor imagesProvider,
            CancellationToken token) =>
            processData.Bind(data => imagesProvider.AddImagesLink(data, token));

        private Task<Result<ScrapTvLibraryData>> UpdateTvLibrary(TvLibraryStorageUpdater libraryStorageUpdater,
            CancellationToken token) => processData
            .Bind(process => libraryStorageUpdater(process.SeriesData.Select(s => s.Series), token)
                .Map(_ => process));
    }
}