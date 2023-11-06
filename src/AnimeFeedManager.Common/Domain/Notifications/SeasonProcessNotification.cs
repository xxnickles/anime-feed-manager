using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Common.Domain.Notifications;

[method: JsonConstructor]
public class SeasonProcessNotification(
    TargetAudience targetAudience,
    NotificationType result,
    SimpleSeasonInfo simpleSeason,
    SeriesType seriesType,
    string message)
    : Notification(targetAudience, result, message)
{
    public SimpleSeasonInfo SimpleSeason { get; set; } = simpleSeason;
    public SeriesType SeriesType { get; set; } = seriesType;

    public void Deconstruct(out TargetAudience targetAudience, out NotificationType result,
        out SimpleSeasonInfo simpleSeason, out SeriesType seriesType, out string message)
    {
        targetAudience = TargetAudience;
        result = Result;
        simpleSeason = SimpleSeason;
        seriesType = SeriesType;
        message = Message;
    }

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, SeasonProcessNotificationContext.Default.SeasonProcessNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SeasonProcessNotification))]
public partial class SeasonProcessNotificationContext : JsonSerializerContext
{
}