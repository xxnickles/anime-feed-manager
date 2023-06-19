namespace AnimeFeedManager.Features.Common.Dto
{
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

    public record EmptyUiNotifications() : UiNotifications(
        System.Array.Empty<UiNotification>(),
        System.Array.Empty<UiNotification>(),
        System.Array.Empty<UiNotification>(),
        System.Array.Empty<UiNotification>(),
        System.Array.Empty<UiNotification>());
}