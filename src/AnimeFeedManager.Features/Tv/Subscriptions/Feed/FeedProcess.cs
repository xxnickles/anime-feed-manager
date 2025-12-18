using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed;

public static class FeedProcess
{
    public static Task<Result<FeedProcessSummary>> RunProcess(
        this Result<DailySeriesFeed[]> dailyFeed,
        TvUserActiveSubscriptions tvUserActiveSubscriptions,
        IDomainPostman domainPostman,
        CancellationToken cancellationToken = default)
    {
        // 1. Go to subs please and get the latest version of the feed
        // This needs to be blocked as we need to wait for the scraping process to be completed
        return dailyFeed
            .InitializeProcess()
            // 2. Add users to notify 
            .AddUsersToNotify(tvUserActiveSubscriptions, cancellationToken)
            // 3. Aggregate notifications
            .AggregateNotifications()
            // 4. Sent them!;
            .SendMessages(domainPostman, cancellationToken);
    }
    
    private static Task<Result<FeedProcessData>> AddUsersToNotify(this Result<FeedProcessData> data, TvUserActiveSubscriptions getter,
        CancellationToken token)
    {
        return data.Bind(d => getter(d.DailyFeed.Select(x => x.Title), token)
            .Map(subscriptions => d with {Subscriptions = subscriptions}));
    }


    private static Result<FeedProcessData> InitializeProcess(this Result<DailySeriesFeed[]> processData)
    {
        return processData.Map(data => new FeedProcessData(data, []));
    }
    
    private static Task<Result<FeedProcessSummary>> SendMessages(
        this Task<Result<FeedNotification[]>> notifications,
        IDomainPostman postman,
        CancellationToken token)
    {
        return notifications.Bind(n => 
            postman.SendMessages(n, token)
                .Map(_ => new FeedProcessSummary(n.Length)));
    }

    extension(Task<Result<FeedProcessData>> data)
    {

        private Task<Result<FeedNotification[]>> AggregateNotifications()
        {
            return data.Map(d => d.Subscriptions.Select(user =>
                {
                    // Create a mapping of series title to notified episodes for this user
                    var notifiedEpisodesByTitle = user.Subscriptions
                        .ToDictionary(
                            sub => sub.SeriesFeedTitle,
                            sub => new HashSet<string>(sub.NotifiedEpisodes));

                    // Filter feeds to user's subscriptions and remove already-notified episodes
                    var relevantFeeds = d.DailyFeed
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
                .ToArray());
        }
    }
}