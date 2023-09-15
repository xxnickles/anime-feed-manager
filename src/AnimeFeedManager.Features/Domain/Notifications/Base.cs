using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

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

public abstract class Notification
{
    protected Notification(
        TargetAudience targetAudience,
        NotificationType result,
        string message)
    {
        TargetAudience = targetAudience;
        Result = result;
        Message = message;
    }

    public TargetAudience TargetAudience { get; set; }
    public NotificationType Result { get; set; }
    public string Message { get; set; }

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