using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <summary>A finished run: toasted, and stored as the latest run for the stat card.</summary>
/// <param name="Queued">Binds sent; each bind's outcome arrives later on its own.</param>
public sealed record AutomatedBindingCompleted(
    DateTimeOffset At,
    int Scheduled,
    int Queued,
    ImmutableArray<ScheduleTie> Ties,
    bool ScheduleSkipped) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.AutomatedBindingCompleted);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        if (ScheduleSkipped)
            return new NotificationComponent("Automated binding skipped",
                builder => builder.AddContent(1, "The SubsPlease schedule was unavailable."));

        return new NotificationComponent("Automated binding finished",
            builder =>
            {
                builder.AddContent(1, Queued == 0
                    ? $"Nothing to bind from {Scheduled} scheduled."
                    : $"{Queued} queued of {Scheduled} scheduled.");
                if (!Ties.IsEmpty)
                    builder.AddContent(2, $" Tied, left unbound: {string.Join(", ", Ties.Select(tie => tie.ShowTitle))}.");
            });
    }
}

public sealed record AutomatedBindingFailed : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.AutomatedBindingFailed);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent("Automated binding failed",
            builder => builder.AddContent(1, "The run did not finish. Check the logs."));
    }
}
