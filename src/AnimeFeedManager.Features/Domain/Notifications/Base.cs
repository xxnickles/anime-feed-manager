using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

public enum TargetAudience
{
    All,
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
    protected Notification(string id,
        TargetAudience targetAudience,
        NotificationType result,
        string message)
    {
        Id = id;
        TargetAudience = targetAudience;
        Result = result;
        Message = message;
    }

    public string Id { get; set; }
    public TargetAudience TargetAudience { get; set; }
    public NotificationType Result { get; set; }
    public string Message { get; set; }

    public void Deconstruct(out string id, out TargetAudience targetAudience, out NotificationType result, out string message)
    {
        id = Id;
        targetAudience = TargetAudience;
        result = Result;
        message = Message;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Notification))]
public partial class NotificationContext : JsonSerializerContext {}