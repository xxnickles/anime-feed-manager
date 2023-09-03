using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

public class AutomatedSubscriptionProcessNotification : Notification
{
    public string[] SubscribedSeries { get; }

    [Newtonsoft.Json.JsonConstructor]
    public AutomatedSubscriptionProcessNotification(
        string id, 
        TargetAudience targetAudience,
        NotificationType result,
        string[] subscribedSeries,
        string message) : base(id, targetAudience, result, message)
    {
        SubscribedSeries = subscribedSeries;
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(AutomatedSubscriptionProcessNotification))]
public partial class AutomatedSubscriptionProcessNotificationContext : JsonSerializerContext {}