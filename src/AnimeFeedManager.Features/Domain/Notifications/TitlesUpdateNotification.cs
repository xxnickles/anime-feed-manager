using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

public class TitlesUpdateNotification : Notification
{
    [JsonConstructor]
    public TitlesUpdateNotification(string id,
        TargetAudience targetAudience,
        NotificationType result,
        string message) : base(id, targetAudience, result, message)
    {
    }


    public new void Deconstruct(out string id, out TargetAudience targetAudience, out NotificationType result,
        out string message)
    {
        id = Id;
        targetAudience = TargetAudience;
        result = Result;
        message = Message;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TitlesUpdateNotification))]
public partial class TitlesUpdateNotificationContext : JsonSerializerContext
{
}