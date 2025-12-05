using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Feed;
using AnimeFeedManager.Features.Tv.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class TriggerFeedNotifications
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly INewReleaseProvider _newReleaseProvider;
    private readonly ILogger<TriggerFeedNotifications> _logger;

    public TriggerFeedNotifications(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        INewReleaseProvider newReleaseProvider,
        ILogger<TriggerFeedNotifications> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _newReleaseProvider = newReleaseProvider;
        _logger = logger;
    }

    [Function(nameof(TriggerFeedNotifications))]
    public async Task Run([TimerTrigger("%FeedNotificationSchedule%")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        await RunProcess(await _newReleaseProvider.Get(), cancellationToken)
            .Match(
                summary => _logger.LogInformation(
                    "{UserCount} users will be notified about new releases. Next run will occur at {Time}",
                    summary.UsersToNotify, myTimer.ScheduleStatus?.Next),
                e => e.LogError(_logger)
            );
    }

    [Function("ManualTriggerFeedNotifications")]
    public async Task RunManual(
        [QueueTrigger(RunFeedNotification.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        RunFeedNotification message, CancellationToken cancellationToken)
    {
        using var tracedActivity = message.StartTracedActivity("ManualTriggerFeedNotifications");
        await RunProcess(await _newReleaseProvider.Get(), cancellationToken)
            .Match(
                summary => _logger.LogInformation(
                    "{UserCount} users will be notified about new releases. This is a manually triggered run",
                    summary.UsersToNotify),
                e => e.LogError(_logger)
            );
    }

    private Task<Result<FeedProcessSummary>> RunProcess(Result<DailySeriesFeed[]> feed, CancellationToken token)
    {
        return feed.RunProcess(
            _tableClientFactory.TableStorageTvUserActiveSubscriptions,
            _domainPostman,
            token);
    }
}