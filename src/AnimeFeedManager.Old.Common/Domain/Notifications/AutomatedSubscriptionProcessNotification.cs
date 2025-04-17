using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Old.Common.Domain.Notifications;

[method: JsonConstructor]
public record AutomatedSubscriptionProcessNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string[] SubscribedSeries,
    string Message)
    : Notification(TargetAudience, Result, Message, Box.Empty())
{
    public string[] SubscribedSeries { get; } = SubscribedSeries;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this,
            AutomatedSubscriptionProcessNotificationContext.Default.AutomatedSubscriptionProcessNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AutomatedSubscriptionProcessNotification))]
public partial class AutomatedSubscriptionProcessNotificationContext : JsonSerializerContext;