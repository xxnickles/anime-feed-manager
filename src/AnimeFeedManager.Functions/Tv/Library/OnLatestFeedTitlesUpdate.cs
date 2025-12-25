using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnLatestFeedTitlesUpdate
{
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
        UpdateLatestFeedTitlesEvent message, CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnLatestFeedTitlesUpdate));
        await RunProcess(token)
            .Match(
                results => _logger.LogInformation(
                    "(OnDemand) Season {Year}-{Season} tv series titles has been updated. {UpdatedSeries} has been updated",
                    results.Season.Year, results.Season.Season, results.UpdatedSeries),
                e => e.LogError(_logger)
            );
    }

    [Function("ScheduledFeedTitlesUpdate")]
    public async Task RunScheduled([TimerTrigger("%FeedTitlesUpdateSchedule%")] TimerInfo myTimer,
        CancellationToken token)
    {
        await RunProcess(token)
            .Match(
                results => _logger.LogInformation(
                    "(Scheduled) Feed titles updated for {Year}-{Season}. {UpdatedSeries} series updated. Next run: {Time}",
                    results.Season.Year, results.Season.Season, results.UpdatedSeries,
                    myTimer.ScheduleStatus?.Next),
                e => e.LogError(_logger)
            );
    }

    public Task<Result<ScrapTvLibraryResult>> RunProcess(CancellationToken token)
    {
        return FeedTitlesScrap.StartFeedUpdateProcess(_tableClientFactory.TableStorageLatestSeason, token)
            .GetFeedTitles(_seasonFeedDataProvider)
            .UpdateSeries(_tableClientFactory.TableStorageRawExistentStoredSeries(),
                _tableClientFactory.TableStorageTvLibraryUpdater, token)
            .SendEvents(_domainPostman, token);
    }
}