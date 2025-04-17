using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Old.Common.Domain.Notifications;

[method: JsonConstructor]
public record ImageUpdateNotification(NotificationType Result, SeriesType SeriesType, string Message)
    : Notification(TargetAudience.Admins, Result, Message, new Box(TargetQueue))
{
    public const string TargetQueue = "image-update-notifications";
    
    public SeriesType SeriesType { get; set; } = SeriesType;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this,
            ImageUpdateNotificationContext.Default.ImageUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ImageUpdateNotification))]
public partial class ImageUpdateNotificationContext : JsonSerializerContext;