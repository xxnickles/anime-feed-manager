using System.Collections.Immutable;

namespace AnimeFeedManager.Application.Notifications;

public sealed record SubscribedFeed(string Title, IImmutableList<TorrentLink> Links, string EpisodeInfo, DateTime PublicationDate);

public sealed record Notification(string Subscriber, IEnumerable<SubscribedFeed> Feeds);