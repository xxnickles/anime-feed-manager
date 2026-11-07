using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.FeedManager.Collection;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Notifications;

public class TriggerFeedNotifications
{
    private static readonly ActivitySource Source = new(Telemetry.TvSubscriptionsFeedSource);

    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly INyaaClient _nyaaClient;
    private readonly WebAppHost _webAppHost;
    private readonly ILogger<TriggerFeedNotifications> _logger;

    public TriggerFeedNotifications(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        INyaaClient nyaaClient,
        WebAppHost webAppHost,
        ILogger<TriggerFeedNotifications> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _nyaaClient = nyaaClient;
        _webAppHost = webAppHost;
        _logger = logger;
    }

    [Function(nameof(TriggerFeedNotifications))]
    public async Task Run([TimerTrigger("%FeedNotificationSchedule%")] TimerInfo myTimer,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(TvSubscriptionsTelemetry.ProcessFeedSpan);
        await RunProcess(cancellationToken)
            .AddLogOnSuccess(run => logger => logger.LogInformation(
                "{notification.user_count} users will be notified about new releases from {nyaa.catalogued_count} catalogued. Next run will occur at {schedule.next_run}",
                run.Summary.UsersToNotify, run.Collected.Catalogued, myTimer.ScheduleStatus?.Next))
            .Complete(_logger);
    }

    [Function("ManualTriggerFeedNotifications")]
    public async Task RunManual(
        [QueueTrigger(RunFeedNotification.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        RunFeedNotification message, FunctionContext functionContext, CancellationToken cancellationToken)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvSubscriptionsTelemetry.ProcessFeedSpan);
        await RunProcess(cancellationToken)
            .AddLogOnSuccess(run => logger => logger.LogInformation(
                "{notification.user_count} users will be notified about new releases from {nyaa.catalogued_count} catalogued. This is a manually triggered run",
                run.Summary.UsersToNotify, run.Collected.Catalogued))
            .Complete(_logger);
    }

    // The poll is stamped only once notifications are sent, so a failed run leaves the previous stamp.
    private async Task<Result<(NyaaCollected Collected, FeedProcessSummary Summary)>> RunProcess(
        CancellationToken token)
    {
        return await NyaaWindow.LastPoll(_tableClientFactory.TableStorageLastNyaaPoll, token)
            .Bind(lastPoll => NyaaCollection.Collect(_nyaaClient, _tableClientFactory, _webAppHost, token)
                .BackfillOnGap(lastPoll, _domainPostman.SendDelayedMessage, token))
            .Bind(collected => Result<DailySeriesFeed[]>.Success([.. collected.Feeds])
                .RunProcess(_tableClientFactory.TableStorageTvUserActiveSubscriptions, _domainPostman.SendMessages,
                    token)
                .Map(summary => (Collected: collected, Summary: summary)))
            .Bind(run => run.Collected.PolledAt is { } polledAt
                ? _tableClientFactory.TableStorageNyaaPollUpdater(new NyaaPoll(polledAt), token).Map(_ => run)
                : Task.FromResult(Result<(NyaaCollected, FeedProcessSummary)>.Success(run)))
            .MarkActivityErroredOnError();
    }
}
