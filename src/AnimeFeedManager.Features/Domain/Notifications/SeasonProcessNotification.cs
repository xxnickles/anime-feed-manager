using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

public class SeasonProcessNotification : Notification
{
    [JsonConstructor]
    public SeasonProcessNotification(string id,
        TargetAudience targetAudience,
        NotificationType result,
        SimpleSeasonInfo simpleSeason,
        SeriesType seriesType,
        string message) : base(id, targetAudience, result, message)
    {
        SimpleSeason = simpleSeason;
        SeriesType = seriesType;
    }

    public SimpleSeasonInfo SimpleSeason { get; set; }
    public SeriesType SeriesType { get; set; }

    public void Deconstruct(out string id, out TargetAudience targetAudience, out NotificationType result, out SimpleSeasonInfo simpleSeason, out SeriesType seriesType, out string message)
    {
        id = Id;
        targetAudience = TargetAudience;
        result = Result;
        simpleSeason = SimpleSeason;
        seriesType = SeriesType;
        message = Message;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SeasonProcessNotification))]
public partial class SeasonProcessNotificationContext : JsonSerializerContext {}