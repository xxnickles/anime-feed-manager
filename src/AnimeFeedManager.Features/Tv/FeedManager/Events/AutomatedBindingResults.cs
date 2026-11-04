using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <param name="Queued">Binds sent; their outcomes arrive as bind events carrying <paramref name="RunId"/>.</param>
public sealed record AutomatedBindingCompleted(
    Guid RunId,
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

        return new NotificationComponent("Automated binding",
            builder =>
            {
                builder.OpenElement(1, "strong");
                builder.AddContent(2, Queued);
                builder.CloseElement();
                builder.AddContent(3, $" queued of {Scheduled} scheduled · ");
                builder.OpenElement(4, "strong");
                builder.AddContent(5, Ties.Length);
                builder.CloseElement();
                builder.AddContent(6, Ties.Length == 1 ? " tie" : " ties");
            });
    }
}

public sealed record AutomatedBindingFailed(Guid RunId) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.AutomatedBindingFailed);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent("Automated binding failed",
            builder => builder.AddContent(1, "No series were bound. Check the logs."));
    }
}
