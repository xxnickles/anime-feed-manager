using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Tv.FeedManager;
using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Functions.Tv.FeedManager;

public class OnAutomatedSeriesBinding
{
    private static readonly ActivitySource Source = new(Telemetry.TvFeedManagerSource);

    private readonly ISubsPleaseSchedule _schedule;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnAutomatedSeriesBinding> _logger;

    public OnAutomatedSeriesBinding(
        ISubsPleaseSchedule schedule,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnAutomatedSeriesBinding> logger)
    {
        _schedule = schedule;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnAutomatedSeriesBinding))]
    public async Task Run(
        [QueueTrigger(RunAutomatedBinding.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        RunAutomatedBinding message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(FeedManagerTelemetry.AutomatedBindingSpan);
        await AutomatedSeriesBinding.Run(_schedule, _tableClientFactory, _domainPostman.SendDelayedMessage, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(summary => logger => logger.LogInformation(
                "(Manual) Automated binding bound {nyaa.bound_count} of {nyaa.scheduled_count} scheduled shows, {nyaa.tie_count} ties",
                summary.Bound, summary.Scheduled, summary.Ties.Length))
            .Complete(_logger);
    }

    [Function("ScheduledAutomatedSeriesBinding")]
    public async Task RunScheduled([TimerTrigger("%AutomatedBindingSchedule%")] TimerInfo timer,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(FeedManagerTelemetry.AutomatedBindingSpan);
        await AutomatedSeriesBinding.Run(_schedule, _tableClientFactory, _domainPostman.SendDelayedMessage, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(summary => logger => logger.LogInformation(
                "(Scheduled) Automated binding bound {nyaa.bound_count} of {nyaa.scheduled_count} scheduled shows, {nyaa.tie_count} ties. Next run: {schedule.next_run}",
                summary.Bound, summary.Scheduled, summary.Ties.Length, timer.ScheduleStatus?.Next))
            .Complete(_logger);
    }
}
