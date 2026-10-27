namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

public sealed record NyaaSeriesBackfilled(
    string SeriesId,
    string Title,
    int Catalogued,
    bool PossiblyTruncated,
    bool Completed) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.NyaaSeriesBackfilled);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent($"{Title} backfilled",
            builder =>
            {
                builder.AddContent(1, $"{Catalogued} releases catalogued.");
                if (PossiblyTruncated)
                    builder.AddContent(2, " The search filled nyaa's 75-item window, so older releases may be missing.");
                if (Completed)
                    builder.AddContent(3, " The feed reached its episode count; the series is completed.");
            })
        {
            Actions = builder =>
            {
                builder.OpenElement(1, "a");
                builder.AddAttribute(2, "href", FeedManagerRoutes.Releases(SeriesId));
                builder.AddAttribute(3, "class", "link text-xs font-semibold");
                builder.AddContent(4, "Open releases");
                builder.CloseElement();
                builder.OpenElement(5, "a");
                builder.AddAttribute(6, "href", FeedManagerRoutes.AdminSeries(SeriesId));
                builder.AddAttribute(7, "class", "link text-xs font-semibold");
                builder.AddContent(8, "Open binding");
                builder.CloseElement();
            }
        };
    }
}

public sealed record NyaaBackfillFailed(string SeriesId) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.NyaaBackfillFailed);
    }

    public override NotificationComponent AsNotificationComponent()
    {
        return new NotificationComponent("Nyaa backfill failed",
            builder =>
            {
                builder.AddContent(1, "The catalog may be incomplete. Check the logs, then run the backfill again.");
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
