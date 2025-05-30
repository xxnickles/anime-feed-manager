using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnTvLibraryUpdate
{
    private readonly ITvLibraryScrapper _scrapper;
    private readonly ITvImagesCollector _imagesCollector;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnTvLibraryUpdate> _logger;

    public OnTvLibraryUpdate(
        ITvLibraryScrapper scrapper,
        ITvImagesCollector imagesCollector,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnTvLibraryUpdate> logger)
    {
        _scrapper = scrapper;
        _imagesCollector = imagesCollector;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnTvLibraryUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateTvSeriesEvent.TargetQueue, Connection = Constants.AzureConnectionName)] UpdateTvSeriesEvent message,
        CancellationToken token)
    {
        await TryGetSeasonSelector(message.SeasonParameters)
            .ScrapTvSeries(_scrapper, token)
            .AddImagesLinks(_imagesCollector, token)
            .UpdateTvLibrary(_tableClientFactory.GetTvLibraryUpdater(), token)
            .SendEvents(_domainPostman, token)
            .Match(
                results => _logger.LogInformation("Season {Year}-{Season} tv series has been updated. {UpdatedSeries} has been updated and {NewSeries} has been added}", results.Season.Year, results.Season.Season, results.UpdatedSeries, results.NewSeries),
                e => e.LogError(_logger)
            );
    }

    private static Result<SeasonSelector> TryGetSeasonSelector(SeasonParameters? season)
    {
        if(season is null)
            return Result<SeasonSelector>.Success(new Latest());

        return (season.Season, season.Year, false)
            .ParseAsSeriesSeason()
            .Map<SeasonSelector>(parsedSeason => new BySeason(parsedSeason.Season, parsedSeason.Year));
    }
}

