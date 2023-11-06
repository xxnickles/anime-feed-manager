using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Domain.Notifications.Base;

public enum TargetAudience
{
    All,
    User,
    Admins
}

public enum NotificationType
{
    Information,
    Update,
    Error
}

public abstract class Notification(
    TargetAudience targetAudience,
    NotificationType result,
    string message)
{
    public TargetAudience TargetAudience { get; set; } = targetAudience;
    public NotificationType Result { get; set; } = result;
    public string Message { get; set; } = message;

    public void Deconstruct(out TargetAudience targetAudience, out NotificationType result, out string message)
    {
        targetAudience = TargetAudience;
        result = Result;
        message = Message;
    }

    public virtual string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, NotificationContext.Default.Notification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Notification))]
public partial class NotificationContext : JsonSerializerContext {}