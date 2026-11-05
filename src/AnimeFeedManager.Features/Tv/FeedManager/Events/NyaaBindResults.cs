namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

public sealed record NyaaSeriesBound(string SeriesId, string Title, string Group, string Variant)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.NyaaSeriesBound);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent($"{Title} bound to {Group}",
            builder =>
            {
                builder.AddContent(1, $"Variant {Variant}. Backfill queued.");
            })
        {
            Actions = builder =>
            {
                builder.OpenElement(1, "a");
                builder.AddAttribute(2, "href", FeedManagerRoutes.AdminSeries(SeriesId));
                builder.AddAttribute(3, "class", "link text-xs font-semibold");
                builder.AddContent(4, "Open binding");
                builder.CloseElement();
            }
        };
    }
}

public sealed record NyaaBindFailed(string SeriesId, string Group, string ReleaseTitle)
    : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.NyaaBindFailed);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent($"Binding [{Group}] {ReleaseTitle} failed",
            builder =>
            {
                builder.AddContent(1, "The series was not bound. Check the logs, then bind again.");
            })
        {
            Actions = builder =>
            {
                builder.OpenElement(1, "a");
                builder.AddAttribute(2, "href", FeedManagerRoutes.AdminSeries(SeriesId));
                builder.AddAttribute(3, "class", "link text-xs font-semibold");
                builder.AddContent(4, "Open binding");
                builder.CloseElement();
            }
        };
    }
}
