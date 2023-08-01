using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

public class ImageUpdateNotification : Notification
{
    [JsonConstructor]
    public ImageUpdateNotification(string id,NotificationType result, SeriesType seriesType, string message) : base(id,
        TargetAudience.Admins, result, message)
    {
        SeriesType = seriesType;
    }

    public SeriesType SeriesType { get; set; }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ImageUpdateNotification))]
public partial class ImageUpdateNotificationContext : JsonSerializerContext {}
