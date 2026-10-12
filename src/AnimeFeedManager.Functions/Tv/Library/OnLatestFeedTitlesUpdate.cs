using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;
using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnLatestFeedTitlesUpdate
{
    private static readonly ActivitySource Source = new(Telemetry.TvLibraryFeedTitlesSource);

    private readonly ITableClientFactory _tableClientFactory;
    private readonly ISeasonFeedDataProvider _seasonFeedDataProvider;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnLatestFeedTitlesUpdate> _logger;

    public OnLatestFeedTitlesUpdate(
        ITableClientFactory tableClientFactory,
        ISeasonFeedDataProvider seasonFeedDataProvider,
        IDomainPostman domainPostman,
        ILogger<OnLatestFeedTitlesUpdate> logger)
    {
        _tableClientFactory = tableClientFactory;
        _seasonFeedDataProvider = seasonFeedDataProvider;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnLatestFeedTitlesUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateLatestFeedTitlesEvent.TargetQueue,
            Connection = StorageRegistrationConstants.QueueConnection)]
        UpdateLatestFeedTitlesEvent message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvLibraryTelemetry.ScrapFeedTitlesSpan);
        await RunProcess(token)
            .AddLogOnSuccess(results => logger => logger.LogInformation(
                "(OnDemand) Season {series.season} tv series titles has been updated. {series.updated_count} has been updated",
                IdHelpers.GenerateAnimePartitionKey(results.Season), results.UpdatedSeries))
            .Complete(_logger);
    }

    [Function("ScheduledFeedTitlesUpdate")]
    public async Task RunScheduled([TimerTrigger("%FeedTitlesUpdateSchedule%")] TimerInfo myTimer,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(TvLibraryTelemetry.ScrapFeedTitlesSpan);
        await RunProcess(token)
            .AddLogOnSuccess(results => logger => logger.LogInformation(
                "(Scheduled) Feed titles updated for {series.season}. {series.updated_count} series updated. Next run: {schedule.next_run}",
                IdHelpers.GenerateAnimePartitionKey(results.Season), results.UpdatedSeries,
                myTimer.ScheduleStatus?.Next))
            .Complete(_logger);
    }

    private async Task<Result<ScrapTvLibraryResult>> RunProcess(CancellationToken token)
    {
        return await FeedTitlesScrap.StartFeedUpdateProcess(_tableClientFactory.TableStorageLatestSeason, token)
            .GetFeedTitles(_seasonFeedDataProvider)
            .UpdateSeries(_tableClientFactory.TableStorageRawExistentStoredSeries(),
                _tableClientFactory.TableStorageTvLibraryUpdater, token)
            .SendEvents(_domainPostman.SendMessages, token)
            .MarkActivityErroredOnError();
    }
}