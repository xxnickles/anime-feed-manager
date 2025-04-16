using System.Text.Json;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Notifications.Types;
using AnimeFeedManager.Features.Tv.Feed;

namespace AnimeFeedManager.Features.Notifications;

public class FeedNotificationsCollector
{
    private readonly IGetNotifications _notificationGetter;

    private record Transport(string Title, FeedDetails[] Details);

    public FeedNotificationsCollector(IGetNotifications notificationGetter)
    {
        _notificationGetter = notificationGetter;
    }

    public Task<Either<DomainError, FeedInformation>> GetUserFeedNotifications(string userId, CancellationToken token)
    {
        return _notificationGetter.GetFeedNotifications(userId, token)
            .MapAsync(items => items.SelectMany(Map))
            .MapAsync(Map);
    }


    private static Transport[] Map(NotificationStorage storage)
    {
        var deserialized = JsonSerializer.Deserialize(storage.Payload ?? string.Empty,
            TvNotificationContext.Default.TvFeedUpdateNotification);

        return deserialized?.Feeds is not null ? Map(deserialized.Feeds) : [];
    }

    private static Transport[] Map(IEnumerable<SubscribedFeed> subscribedFeeds)
    {
        return subscribedFeeds.GroupBy(s => s.Title).Select(g =>
            new Transport(g.Key, g.Select(i => new FeedDetails(i.Links, i.EpisodeInfo)).ToArray())).ToArray();
    }

    private static FeedInformation Map(IEnumerable<Transport> items)
    {
        var dictionary = items.GroupBy(s => s.Title)
            .ToDictionary(g =>
                    g.Key,
                g => g.SelectMany(i => i.Details).ToArray());

        return new FeedInformation(dictionary);
    }
}