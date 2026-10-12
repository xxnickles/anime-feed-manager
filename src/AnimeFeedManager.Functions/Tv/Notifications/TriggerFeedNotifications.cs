using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class TriggerFeedNotifications
{
    private static readonly ActivitySource Source = new(Telemetry.TvSubscriptionsFeedSource);

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
        using var activity = Source.StartActivity(TvSubscriptionsTelemetry.ProcessFeedSpan);
        await RunProcess(await _newReleaseProvider.Get(), cancellationToken)
            .AddLogOnSuccess(summary => logger => logger.LogInformation(
                "{notification.user_count} users will be notified about new releases. Next run will occur at {schedule.next_run}",
                summary.UsersToNotify, myTimer.ScheduleStatus?.Next))
            .Complete(_logger);
    }

    [Function("ManualTriggerFeedNotifications")]
    public async Task RunManual(
        [QueueTrigger(RunFeedNotification.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        RunFeedNotification message, FunctionContext functionContext, CancellationToken cancellationToken)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvSubscriptionsTelemetry.ProcessFeedSpan);
        await RunProcess(await _newReleaseProvider.Get(), cancellationToken)
            .AddLogOnSuccess(summary => logger => logger.LogInformation(
                "{notification.user_count} users will be notified about new releases. This is a manually triggered run",
                summary.UsersToNotify))
            .Complete(_logger);
    }

    private async Task<Result<FeedProcessSummary>> RunProcess(Result<DailySeriesFeed[]> feed, CancellationToken token)
    {
        return await feed.RunProcess(
            _tableClientFactory.TableStorageTvUserActiveSubscriptions,
            _domainPostman.SendMessages,
            token)
            .MarkActivityErroredOnError();
    }
}