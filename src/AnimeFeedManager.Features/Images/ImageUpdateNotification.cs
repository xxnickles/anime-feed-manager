using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Features.Domain.Notifications;

namespace AnimeFeedManager.Features.Images;

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