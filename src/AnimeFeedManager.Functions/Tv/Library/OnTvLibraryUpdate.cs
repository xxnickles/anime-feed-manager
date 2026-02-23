using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using static AnimeFeedManager.Features.Tv.Library.ScrapProcess.TvScrapProcess;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnTvLibraryUpdate
{
    private readonly ITvLibraryScrapper _scrapper;
    private readonly IImageProvider _imageProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnTvLibraryUpdate> _logger;

    public OnTvLibraryUpdate(
        ITvLibraryScrapper scrapper,
        IImageProvider imageProvider,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnTvLibraryUpdate> logger)
    {
        _scrapper = scrapper;
        _imageProvider = imageProvider;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnTvLibraryUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateTvSeriesEvent.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        UpdateTvSeriesEvent message,
        CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnTvLibraryUpdate));
        await RunScraper(message.SeasonParameters, token)
            .AddLogOnSuccess(results => logger => logger.LogInformation(
                "(OnDemand) Season {Year}-{Season} tv series has been updated. {UpdatedSeries} has been updated and {NewSeries} has been added",
                results.Season.Year,
                results.Season.Season,
                results.UpdatedSeries,
                results.NewSeries))
            .Complete(_logger);
    }

    [Function("ScheduledTvLibraryUpdate")]
    public async Task RunScheduled([TimerTrigger("%ScrapingSchedule%")] TimerInfo myTimer,
        CancellationToken token)
    {
        await RunScraper(null, token)
            .AddLogOnSuccess(results => logger => logger.LogInformation(
                "(Scheduled) Season {Year}-{Season} tv series has been updated. {UpdatedSeries} has been updated and {NewSeries} has been added. Next run will happen at {Time}",
                results.Season.Year,
                results.Season.Season,
                results.UpdatedSeries,
                results.NewSeries,
                myTimer.ScheduleStatus?.Next))
            .Complete(_logger);
    }

    private Task<Result<ScrapTvLibraryResult>> RunScraper(
        SeasonParameters? seasonParameters,
        CancellationToken token) =>
        ScrapTvSeries(
                seasonParameters,
                _scrapper.ScrapTvSeries,
                _imageProvider.Process,
                _tableClientFactory.TableStorageTvLibraryUpdater,
                token)
            .SendEvents(_domainPostman.SendMessages, seasonParameters, token);


    private static Result<SeasonSelector> TryGetSeasonSelector(SeasonParameters? season)
    {
        if (season is null)
            return new Latest();

        return (season.Season, season.Year, false)
            .ParseAsSeriesSeason()
            .Map<SeriesSeason, SeasonSelector>(parsedSeason => new BySeason(parsedSeason.Season, parsedSeason.Year));
    }
}