using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public static class TvScrapProcess
{
    public static Task<Result<ScrapTvLibraryData, DomainError>> ScrapTvSeries(
        SeasonParameters? season,
        TvScrapper scrapper,
        ImageProcessor imagesProvider,
        TvLibraryStorageUpdater libraryStorageUpdater,
        CancellationToken token)
    {
        return TryGetSeasonSelector(season)
            .MapError(error => (DomainError)error)
            .GetTvSeries(scrapper, token)
            .AddImagesLinks(imagesProvider, token)
            .UpdateTvLibrary(libraryStorageUpdater, token);
    }
    
    // Just combinators to make the code more readable
    
    private static Result<SeasonSelector, ValidationError.Fields> TryGetSeasonSelector(SeasonParameters? season)
    {
        if (season is null)
            return new Current();

        return (season.Season, season.Year, false)
            .ParseAsSeriesSeason()
            .Map(SeasonSelector (parsedSeason) => new BySeason(parsedSeason.Season, parsedSeason.Year));
    }
    
    
    private static Task<Result<ScrapTvLibraryData, DomainError>> GetTvSeries(
        this Result<SeasonSelector, DomainError> seasonSelector,
        TvScrapper scrapper,
        CancellationToken token) => seasonSelector.Bind(season => scrapper(season, token));
    
   
    extension(Task<Result<ScrapTvLibraryData, DomainError>> processData)
    {
        private Task<Result<ScrapTvLibraryData, DomainError>> AddImagesLinks(ImageProcessor imagesProvider,
            CancellationToken token) =>
            processData.Bind(data => imagesProvider.AddImagesLink(data, token));

        private Task<Result<ScrapTvLibraryData, DomainError>> UpdateTvLibrary(TvLibraryStorageUpdater libraryStorageUpdater,
            CancellationToken token) => processData
            .Bind(process => libraryStorageUpdater(process.SeriesData.Select(s => s.Series), token)
                .Map(_ => process));
    }
}