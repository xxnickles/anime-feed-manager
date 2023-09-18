using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Seasons;

public class SeasonProcessNotification : Notification
{
    [JsonConstructor]
    public SeasonProcessNotification(
        TargetAudience targetAudience,
        NotificationType result,
        SimpleSeasonInfo simpleSeason,
        SeriesType seriesType,
        string message) : base(targetAudience, result, message)
    {
        SimpleSeason = simpleSeason;
        SeriesType = seriesType;
    }

    public SimpleSeasonInfo SimpleSeason { get; set; }
    public SeriesType SeriesType { get; set; }

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