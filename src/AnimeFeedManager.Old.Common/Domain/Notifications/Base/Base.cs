using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Events;

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

public abstract record Notification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message, 
    Box MessageBox) : DomainMessage(MessageBox)
{
    public TargetAudience TargetAudience { get; set; } = TargetAudience;
    public NotificationType Result { get; set; } = Result;
    public string Message { get; set; } = Message;

    public virtual string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, NotificationContext.Default.Notification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Notification))]
public partial class NotificationContext : JsonSerializerContext;