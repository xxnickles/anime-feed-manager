namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

public record NotificationSent(string Title, string Url, string[] Episodes) : SystemNotificationPayload
{
    public override string AsJson()
    {
        return JsonSerializer.Serialize(this, NotificationSentContext.Default.NotificationSent);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(NotificationSent))]
public partial class NotificationSentContext : JsonSerializerContext;