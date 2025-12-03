using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Feed.Events;

public sealed record FeedNotification(UserActiveSubscriptions Subscriptions, DailySeriesFeed[] Feeds) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-feed-notification";
    public override BinaryData ToBinaryData()
    {
      return BinaryData.FromObjectAsJson(this, FeedNotificationContext.Default.FeedNotification);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(FeedNotification))]
public partial class FeedNotificationContext : JsonSerializerContext;