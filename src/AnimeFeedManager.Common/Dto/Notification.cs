using System.Collections.Immutable;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Common.Dto;

public sealed record SubscribedFeed(string Title, IImmutableList<TorrentLink> Links, string EpisodeInfo, DateTime PublicationDate);

public sealed record Notification(string Subscriber, IEnumerable<SubscribedFeed> Feeds);

public sealed record NotifiedTitle(string Subscriber, string Title);


public readonly record struct UiNotification(NotificationType Type, string Payload);

public readonly record struct UiNotifications(
    UiNotification[] TvNotifications,
    UiNotification[] TvTitlesNotifications,
    UiNotification[] OvasNotifications,
    UiNotification[] MoviesNotifications,
    UiNotification[] ImagesNotifications,
    UiNotification[] AdminNotifications);
