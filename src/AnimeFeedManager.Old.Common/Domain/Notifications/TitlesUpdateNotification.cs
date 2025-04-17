using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Old.Common.Domain.Notifications;

[method: JsonConstructor]
public record TitlesUpdateNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message)
    : Notification(TargetAudience, Result, Message, new Box(TargetQueue))
{
    public const string TargetQueue = "title-update-notifications";
    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, TitlesUpdateNotificationContext.Default.TitlesUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TitlesUpdateNotification))]
public partial class TitlesUpdateNotificationContext : JsonSerializerContext;