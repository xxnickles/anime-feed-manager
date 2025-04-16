using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Common.Dto;

public record SubscribedFeed(string Title, TorrentLink[] Links, string EpisodeInfo, DateTime PublicationDate);

public record SubscriberTvNotification(string Subscriber, string SubscriberId, SubscribedFeed[] Feeds)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-notifications";
}