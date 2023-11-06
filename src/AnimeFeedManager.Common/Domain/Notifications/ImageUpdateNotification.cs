using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Common.Domain.Notifications;

[method: JsonConstructor]
public class ImageUpdateNotification(NotificationType result, SeriesType seriesType, string message)
    : Notification(TargetAudience.Admins, result, message)
{
    public SeriesType SeriesType { get; set; } = seriesType;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this,
            ImageUpdateNotificationContext.Default.ImageUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ImageUpdateNotification))]
public partial class ImageUpdateNotificationContext : JsonSerializerContext
{
}