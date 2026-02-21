namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

public record NotificationSent(string Title, string Url, string[] Episodes) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, TvJsonContext.Default.NotificationSent);
    }
}
