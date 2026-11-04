using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public sealed record FailedBind(string SeriesId, string ReleaseTitle);

public abstract record LastAutomatedBinding;

public sealed record NoAutomatedBinding : LastAutomatedBinding;

public sealed record LatestAutomatedBinding(
    DateTimeOffset At,
    int Scheduled,
    int Queued,
    int Bound,
    ImmutableArray<FailedBind> Failed,
    ImmutableArray<ScheduleTie> Ties,
    bool ScheduleSkipped) : LastAutomatedBinding
{
    /// <summary>Queued binds without an outcome yet: still paced, or lost.</summary>
    public int Pending => Math.Max(0, Queued - Bound - Failed.Length);
}

public static class AutomatedBindingStat
{
    public static Task<Result<LastAutomatedBinding>> Get(
        SystemEventsGetter<AutomatedBindingCompleted> runGetter,
        SystemEventsGetter<NyaaSeriesBound> boundGetter,
        SystemEventsGetter<NyaaBindFailed> failedGetter,
        DateTimeOffset from,
        CancellationToken cancellationToken)
    {
        var consumer = TargetConsumer.Admin();
        return runGetter(consumer, from, cancellationToken)
            .Bind(runs => boundGetter(consumer, from, cancellationToken)
                .Bind(bound => failedGetter(consumer, from, cancellationToken)
                    .Map(failed => Summarize(runs, bound, failed))));
    }

    internal static LastAutomatedBinding Summarize(
        ImmutableArray<EventData<AutomatedBindingCompleted>> runs,
        ImmutableArray<EventData<NyaaSeriesBound>> bound,
        ImmutableArray<EventData<NyaaBindFailed>> failed)
    {
        if (runs.IsEmpty) return new NoAutomatedBinding();

        var latest = runs.MaxBy(e => e.Timestamp)!;
        var run = latest.Data;
        return new LatestAutomatedBinding(
            latest.Timestamp,
            run.Scheduled,
            run.Queued,
            bound.Count(e => e.Data.RunId == run.RunId),
            [
                .. failed
                    .Where(e => e.Data.RunId == run.RunId)
                    .Select(e => new FailedBind(e.Data.SeriesId, e.Data.ReleaseTitle))
            ],
            run.Ties,
            run.ScheduleSkipped);
    }
}
