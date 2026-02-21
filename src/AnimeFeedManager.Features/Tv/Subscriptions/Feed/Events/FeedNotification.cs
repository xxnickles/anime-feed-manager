using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

public sealed record FeedNotification(UserActiveSubscriptions Subscriptions, DailySeriesFeed[] Feeds) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-feed-notification";
    public override BinaryData ToBinaryData()
    {
      return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.FeedNotification);
    }
}
