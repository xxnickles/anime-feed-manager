using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Common.Dto;

public sealed record SubscribedFeed(string Title, TorrentLink[] Links, string EpisodeInfo, DateTime PublicationDate);

public sealed record Notification(string Subscriber, IEnumerable<SubscribedFeed> Feeds);

public sealed record NotifiedTitle(string Subscriber, string Title);


public readonly record struct UiNotification(string Type, DateTimeOffset TimeOffset, string Payload);

public record UiNotifications(
    UiNotification[] TvNotifications,
    UiNotification[] OvasNotifications,
    UiNotification[] MoviesNotifications,
    UiNotification[] ImagesNotifications,
    UiNotification[] AdminNotifications);

public record EmptyUINotifications() : UiNotifications(
    Array.Empty<UiNotification>(),
    Array.Empty<UiNotification>(),
    Array.Empty<UiNotification>(),
    Array.Empty<UiNotification>(),
    Array.Empty<UiNotification>());
