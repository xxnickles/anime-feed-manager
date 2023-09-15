using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnimeFeedManager.Features.Domain.Notifications;

public class AutomatedSubscriptionProcessNotification : Notification
{
    public string[] SubscribedSeries { get; }

    [JsonConstructor]
    public AutomatedSubscriptionProcessNotification(
        TargetAudience targetAudience,
        NotificationType result,
        string[] subscribedSeries,
        string message) : base(targetAudience, result, message)
    {
        SubscribedSeries = subscribedSeries;
    }

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