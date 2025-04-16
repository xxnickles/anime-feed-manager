using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;

namespace AnimeFeedManager.Features.Movies.Subscriptions.Types;

[method: JsonConstructor]
public record MoviesFeedSentNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message,
    DateTime Time,
    IEnumerable<FeedProcessedMovie> Feeds)
    : Notification(TargetAudience, Result, Message, Box.Empty())
{
    public DateTime Time { get; } = Time;
    public IEnumerable<FeedProcessedMovie> Feeds { get; } = Feeds;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this, MoviesFeedSentNotificationContext.Default.MoviesFeedSentNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(MoviesFeedSentNotification))]
public partial class MoviesFeedSentNotificationContext : JsonSerializerContext;