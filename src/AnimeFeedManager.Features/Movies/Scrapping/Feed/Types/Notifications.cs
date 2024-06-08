using System.Text.Json;
using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;

[method: JsonConstructor]
public record MoviesFeedUpdateNotification(
    NotificationType Result,
    SeriesType SeriesType,
    string SeasonInfo,
    string Message)
    : Notification(TargetAudience.Admins, Result, Message, new Box(TargetQueue))
{
    public const string TargetQueue = "movies-feed-update-notifications";

    public SeriesType SeriesType { get; set; } = SeriesType;

    public string SeasonInfo { get; set; } = string.Empty;

    public override string GetSerializedPayload()
    {
        return JsonSerializer.Serialize(this,
            MoviesFeedUpdateNotificationContext.Default.MoviesFeedUpdateNotification);
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(MoviesFeedUpdateNotification))]
public partial class MoviesFeedUpdateNotificationContext : JsonSerializerContext
{
}