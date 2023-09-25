using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Features.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.Common.Domain.Notifications;

public class TitlesUpdateNotification : Notification
{
    [JsonConstructor]
    public TitlesUpdateNotification(
        TargetAudience targetAudience,
        NotificationType result,
        string message) : base(targetAudience, result, message)
    {
    }

    public new void Deconstruct(out TargetAudience targetAudience, out NotificationType result,
        out string message)
    {
        targetAudience = TargetAudience;
        result = Result;
        message = Message;
    }

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, TitlesUpdateNotificationContext.Default.TitlesUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TitlesUpdateNotification))]
public partial class TitlesUpdateNotificationContext : JsonSerializerContext
{
}