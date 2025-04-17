using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;

namespace AnimeFeedManager.Old.Features.Ovas.Subscriptions.Types;

[method: JsonConstructor]
public record OvasFeedSentNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message,
    DateTime Time,
    IEnumerable<FeedProcessedOva> Feeds)
    : Notification(TargetAudience, Result, Message, Box.Empty())
{
    public DateTime Time { get; } = Time;
    public IEnumerable<FeedProcessedOva> Feeds { get; } = Feeds;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, OvasFeedSentNotificationContext.Default.OvasFeedSentNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(OvasFeedSentNotification))]
public partial class OvasFeedSentNotificationContext : JsonSerializerContext;