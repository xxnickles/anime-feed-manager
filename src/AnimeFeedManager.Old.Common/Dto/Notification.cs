using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Types;

namespace AnimeFeedManager.Old.Common.Dto;

public record SubscribedFeed(string Title, TorrentLink[] Links, string EpisodeInfo, DateTime PublicationDate);

public record SubscriberTvNotification(string Subscriber, string SubscriberId, SubscribedFeed[] Feeds)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-notifications";
}