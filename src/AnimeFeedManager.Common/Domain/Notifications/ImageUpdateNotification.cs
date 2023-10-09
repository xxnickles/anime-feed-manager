using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Common.Domain.Notifications;

public class ImageUpdateNotification : Notification
{
    [JsonConstructor]
    public ImageUpdateNotification(NotificationType result, SeriesType seriesType, string message) : base(
        TargetAudience.Admins, result, message)
    {
        SeriesType = seriesType;
    }

    public SeriesType SeriesType { get; set; }

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