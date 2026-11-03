using AnimeFeedManager.Features.Scrapping.SubsPlease;

namespace AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

public sealed record AutomatedBindingSummary(int Scheduled);

internal sealed record AutomatedBindingProcess(ImmutableArray<ScheduledShow> Schedule);

public static class AutomatedSeriesBinding
{
    /// <summary>Binds series from the SubsPlease schedule; a failed or empty schedule skips binding with a warning.</summary>
    public static Task<Result<AutomatedBindingSummary>> Run(ISubsPleaseSchedule schedule, CancellationToken token) =>
        StartProcess(schedule, token)
            .Map(process => new AutomatedBindingSummary(process.Schedule.Length))
            .WithOperationName(nameof(Run));

    private static Task<Result<AutomatedBindingProcess>> StartProcess(ISubsPleaseSchedule schedule,
        CancellationToken token) =>
        schedule.Fetch(token)
            .Map(shows => new AutomatedBindingProcess(shows))
            .BindOnError(error => Result<AutomatedBindingProcess>.Success(new AutomatedBindingProcess([]))
                .AddLog(ScheduleSkipped(error)));

    private static Action<ILogger> ScheduleSkipped(DomainError error) =>
        logger => logger.LogWarning(
            "SubsPlease schedule unavailable, automated binding skipped: {domain_error.message}", error.Message);
}
