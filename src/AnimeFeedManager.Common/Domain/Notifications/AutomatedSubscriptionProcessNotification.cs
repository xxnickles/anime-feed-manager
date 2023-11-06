using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Common.Domain.Notifications;

[method: JsonConstructor]
public class AutomatedSubscriptionProcessNotification(
    TargetAudience targetAudience,
    NotificationType result,
    string[] subscribedSeries,
    string message)
    : Notification(targetAudience, result, message)
{
    public string[] SubscribedSeries { get; } = subscribedSeries;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this,
            AutomatedSubscriptionProcessNotificationContext.Default.AutomatedSubscriptionProcessNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AutomatedSubscriptionProcessNotification))]
public partial class AutomatedSubscriptionProcessNotificationContext : JsonSerializerContext
{
}