namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

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
