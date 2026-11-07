using AnimeFeedManager.Features.Tv.FeedManager;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Settlement;

namespace AnimeFeedManager.Functions.Tv.FeedManager;

public class OnFeedSettlement
{
    private static readonly ActivitySource Source = new(Telemetry.TvFeedManagerSource);

    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnFeedSettlement> _logger;

    public OnFeedSettlement(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnFeedSettlement> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnFeedSettlement))]
    public async Task Run(
        [QueueTrigger(RunFeedSettlement.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        RunFeedSettlement message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(FeedManagerTelemetry.SettleFeedsSpan);
        await SettleNyaaFeeds.Run(_tableClientFactory, _domainPostman.SendMessages, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(settled => logger => logger.LogInformation(
                "(Manual) Feed settlement stamped {nyaa.stamped_count} completed feeds, retired {nyaa.retired_count}",
                settled.Stamped, settled.Retired))
            .Complete(_logger);
    }

    [Function("ScheduledFeedSettlement")]
    public async Task RunScheduled([TimerTrigger("%FeedSettlementSchedule%")] TimerInfo timer,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(FeedManagerTelemetry.SettleFeedsSpan);
        await SettleNyaaFeeds.Run(_tableClientFactory, _domainPostman.SendMessages, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(settled => logger => logger.LogInformation(
                "(Scheduled) Feed settlement stamped {nyaa.stamped_count} completed feeds, retired {nyaa.retired_count}. Next run: {schedule.next_run}",
                settled.Stamped, settled.Retired, timer.ScheduleStatus?.Next))
            .Complete(_logger);
    }
}
