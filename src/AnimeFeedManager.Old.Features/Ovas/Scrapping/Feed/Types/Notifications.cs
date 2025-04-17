using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.Types;

[method: JsonConstructor]
public record OvasFeedUpdateNotification(
    NotificationType Result,
    SeriesType SeriesType,
    string SeasonInfo,
    string Message)
    : Notification(TargetAudience.Admins, Result, Message, new Box(TargetQueue))
{
    public const string TargetQueue = "ova-feed-update-notifications";

    public SeriesType SeriesType { get; set; } = SeriesType;

    public string SeasonInfo { get; set; } = string.Empty;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this,
            OvasFeedUpdateNotificationContext.Default.OvasFeedUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(OvasFeedUpdateNotification))]
public partial class OvasFeedUpdateNotificationContext : JsonSerializerContext;