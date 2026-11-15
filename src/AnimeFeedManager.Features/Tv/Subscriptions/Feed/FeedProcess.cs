using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed;

[Widens]
public partial union FeedProcessError(StorageError, DeliveryError);

public static class FeedProcess
{
    /// <summary>Notifies each subscribed user of the feed episodes they haven't been notified about yet.</summary>
    public static Task<Result<FeedProcessSummary, FeedProcessError>> RunProcess(
        DailySeriesFeed[] dailyFeed,
        TvUserActiveSubscriptions tvUserActiveSubscriptions,
        DomainCollectionSender domainPostman,
        CancellationToken cancellationToken = default) =>
        tvUserActiveSubscriptions(dailyFeed.Select(feed => feed.Title), cancellationToken)
            .MapError(error => (FeedProcessError)error)
            .Map(users => Notifications(dailyFeed, users))
            .Bind(notifications => domainPostman(notifications, cancellationToken)
                .Map(_ => new FeedProcessSummary(notifications.Length)));

    private static FeedNotification[] Notifications(DailySeriesFeed[] dailyFeed, UserActiveSubscriptions[] users) =>
        users.Select(user =>
            {
                var notifiedEpisodesByTitle = user.Subscriptions
                    .ToDictionary(
                        sub => sub.SeriesFeedTitle,
                        sub => new HashSet<string>(sub.NotifiedEpisodes));

                // The user's feeds, without the episodes they were already notified about
                var relevantFeeds = dailyFeed
                    .Where(feed => notifiedEpisodesByTitle.ContainsKey(feed.Title))
                    .Select(feed =>
                    {
                        var notifiedEpisodes = notifiedEpisodesByTitle[feed.Title];
                        var newEpisodes = feed.Episodes
                            .Where(ep => !notifiedEpisodes.Contains(ep.EpisodeNumber))
                            .ToArray();

                        return new { Feed = feed, NewEpisodes = newEpisodes };
                    })
                    .Where(x => x.NewEpisodes.Length > 0)
                    .Select(x => x.Feed with {Episodes = x.NewEpisodes})
                    .ToArray();

                return new {User = user, Feeds = relevantFeeds};
            })
            .Where(pair => pair.Feeds.Length > 0)
            .Select(pair => new FeedNotification(pair.User, pair.Feeds))
            .ToArray();
}
