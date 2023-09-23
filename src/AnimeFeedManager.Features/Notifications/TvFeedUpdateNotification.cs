using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Features.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.Notifications;

public class TvFeedUpdateNotification : Notification
{
    public DateTime Time { get; }
    public IEnumerable<SubscribedFeed> Feeds { get; }

    [JsonConstructor]
    public TvFeedUpdateNotification(
        TargetAudience targetAudience, 
        NotificationType result,
        string message,
        DateTime time, 
        IEnumerable<SubscribedFeed> feeds) : base(targetAudience, result, message)
    {
        Time = time;
        Feeds = feeds;
    }

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, TvNotificationContext.Default.TvFeedUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(TvFeedUpdateNotification))]
public partial class TvNotificationContext : JsonSerializerContext
{
}