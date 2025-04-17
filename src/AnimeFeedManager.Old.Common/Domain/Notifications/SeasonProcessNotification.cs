using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Dto;

namespace AnimeFeedManager.Old.Common.Domain.Notifications;

[method: JsonConstructor]
public record SeasonProcessNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    SimpleSeasonInfo SimpleSeason,
    SeriesType SeriesType,
    string Message)
    : Notification(TargetAudience, Result, Message, new Box(TargetQueue))
{
    public const string TargetQueue = "season-process-notifications";
    public SimpleSeasonInfo SimpleSeason { get; set; } = SimpleSeason;
    public SeriesType SeriesType { get; set; } = SeriesType;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, SeasonProcessNotificationContext.Default.SeasonProcessNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SeasonProcessNotification))]
public partial class SeasonProcessNotificationContext : JsonSerializerContext;