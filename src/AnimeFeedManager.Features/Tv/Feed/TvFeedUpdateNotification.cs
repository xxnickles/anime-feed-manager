using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.Notifications;

[method: JsonConstructor]
public record TvFeedUpdateNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message,
    DateTime Time,
    IEnumerable<SubscribedFeed> Feeds)
    : Notification(TargetAudience, Result, Message, Box.Empty())
{
    public DateTime Time { get; } = Time;
    public IEnumerable<SubscribedFeed> Feeds { get; } = Feeds;

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