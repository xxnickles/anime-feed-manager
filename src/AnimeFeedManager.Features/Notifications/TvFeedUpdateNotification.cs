using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.Notifications;

[method: JsonConstructor]
public class TvFeedUpdateNotification(
    TargetAudience targetAudience,
    NotificationType result,
    string message,
    DateTime time,
    IEnumerable<SubscribedFeed> feeds)
    : Notification(targetAudience, result, message)
{
    public DateTime Time { get; } = time;
    public IEnumerable<SubscribedFeed> Feeds { get; } = feeds;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, TvNotificationContext.Default.TvFeedUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TvFeedUpdateNotification))]
public partial class TvNotificationContext : JsonSerializerContext
{
}