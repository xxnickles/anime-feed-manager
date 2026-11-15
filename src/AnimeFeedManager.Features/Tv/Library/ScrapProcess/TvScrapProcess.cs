using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

[Widens<TvScrapError>]
public partial union ScrapProcessError(UnreadableMessage, StorageError, NoLatestSeason, ScrapeError, ImageNotStored, DeliveryError);

public static class TvScrapProcess
{
    public static Task<Result<ScrapTvLibraryData, ScrapProcessError>> ScrapTvSeries(
        SeasonParameters? season,
        TvScrapper scrapper,
        ImageProcessor imagesProvider,
        TvLibraryStorageUpdater libraryStorageUpdater,
        CancellationToken token)
    {
        return TryGetSeasonSelector(season)
            .MapError(ScrapProcessError (fields) => new UnreadableMessage(typeof(UpdateTvSeriesEvent), fields.Errors))
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
    
    
    private static Task<Result<ScrapTvLibraryData, ScrapProcessError>> GetTvSeries(
        this Result<SeasonSelector, ScrapProcessError> seasonSelector,
        TvScrapper scrapper,
        CancellationToken token) => seasonSelector.Bind(season => scrapper(season, token));
    
   
    extension(Task<Result<ScrapTvLibraryData, ScrapProcessError>> processData)
    {
        private Task<Result<ScrapTvLibraryData, ScrapProcessError>> AddImagesLinks(ImageProcessor imagesProvider,
            CancellationToken token) =>
            processData.Bind(data => imagesProvider.AddImagesLink(data, token));

        private Task<Result<ScrapTvLibraryData, ScrapProcessError>> UpdateTvLibrary(TvLibraryStorageUpdater libraryStorageUpdater,
            CancellationToken token) => processData
            .Bind(process => libraryStorageUpdater(process.SeriesData.Select(s => s.Series), token)
                .Map(_ => process));
    }
}