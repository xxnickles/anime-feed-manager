using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;
using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using static AnimeFeedManager.Features.Tv.Library.ScrapProcess.TvScrapProcess;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnTvLibraryUpdate
{
    private static readonly ActivitySource Source = new(Telemetry.TvLibraryImportSource);

    private readonly IAnimeScheduleClient _animeScheduleClient;
    private readonly ISeasonFeedDataProvider _seasonFeedDataProvider;
    private readonly IImageProvider _imageProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnTvLibraryUpdate> _logger;

    public OnTvLibraryUpdate(
        IAnimeScheduleClient animeScheduleClient,
        ISeasonFeedDataProvider seasonFeedDataProvider,
        IImageProvider imageProvider,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnTvLibraryUpdate> logger)
    {
        _animeScheduleClient = animeScheduleClient;
        _seasonFeedDataProvider = seasonFeedDataProvider;
        _imageProvider = imageProvider;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnTvLibraryUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateTvSeriesEvent.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        UpdateTvSeriesEvent message,
        FunctionContext functionContext,
        CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvLibraryTelemetry.ImportSpan);
        await RunScraper(message.SeasonParameters, token)
            .AddLogOnSuccess(results => logger => logger.LogInformation(
                "(OnDemand) Season {series.season} tv series has been updated. {series.updated_count} has been updated and {series.new_count} has been added",
                IdHelpers.GenerateAnimePartitionKey(results.Season),
                results.UpdatedSeries,
                results.NewSeries))
            .Complete(_logger);
    }

    [Function("ScheduledTvLibraryUpdate")]
    public async Task RunScheduled([TimerTrigger("%ScrapingSchedule%")] TimerInfo myTimer,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(TvLibraryTelemetry.ImportSpan);
        await RunScraper(null, token)
            .AddLogOnSuccess(results => logger => logger.LogInformation(
                "(Scheduled) Season {series.season} tv series has been updated. {series.updated_count} has been updated and {series.new_count} has been added. Next run will happen at {schedule.next_run}",
                IdHelpers.GenerateAnimePartitionKey(results.Season),
                results.UpdatedSeries,
                results.NewSeries,
                myTimer.ScheduleStatus?.Next))
            .Complete(_logger);
    }

    private async Task<Result<ScrapTvLibraryResult>> RunScraper(
        SeasonParameters? seasonParameters,
        CancellationToken token)
    {
        return await ScrapTvSeries(
                seasonParameters,
                TvLibraryScrapper.Scrapper(
                    _animeScheduleClient,
                    _seasonFeedDataProvider,
                    _tableClientFactory.TableStorageLatestSeason,
                    _tableClientFactory.TableStorageExistentStoredSeriesGetter),
                _imageProvider.Process,
                _tableClientFactory.TableStorageTvLibraryUpdater,
                token)
            .SendEvents(_domainPostman.SendMessages, seasonParameters, token)
            .MarkActivityErroredOnError();
    }
}