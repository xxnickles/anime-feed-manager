using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Tv.FeedManager;
using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Functions.Tv.FeedManager;

public class OnAutomatedSeriesBinding
{
    private static readonly ActivitySource Source = new(Telemetry.TvFeedManagerSource);

    private readonly ISubsPleaseSchedule _schedule;
    private readonly ILogger<OnAutomatedSeriesBinding> _logger;

    public OnAutomatedSeriesBinding(
        ISubsPleaseSchedule schedule,
        ILogger<OnAutomatedSeriesBinding> logger)
    {
        _schedule = schedule;
        _logger = logger;
    }

    [Function(nameof(OnAutomatedSeriesBinding))]
    public async Task Run(
        [QueueTrigger(RunAutomatedBinding.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        RunAutomatedBinding message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(FeedManagerTelemetry.AutomatedBindingSpan);
        await AutomatedSeriesBinding.Run(_schedule, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(summary => logger => logger.LogInformation(
                "(Manual) Automated binding read {nyaa.scheduled_count} scheduled shows",
                summary.Scheduled))
            .Complete(_logger);
    }

    [Function("ScheduledAutomatedSeriesBinding")]
    public async Task RunScheduled([TimerTrigger("%AutomatedBindingSchedule%")] TimerInfo timer,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(FeedManagerTelemetry.AutomatedBindingSpan);
        await AutomatedSeriesBinding.Run(_schedule, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(summary => logger => logger.LogInformation(
                "(Scheduled) Automated binding read {nyaa.scheduled_count} scheduled shows. Next run: {schedule.next_run}",
                summary.Scheduled, timer.ScheduleStatus?.Next))
            .Complete(_logger);
    }
}
