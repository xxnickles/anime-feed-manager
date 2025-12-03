using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Feed.Events;
using AnimeFeedManager.Features.Tv.Feed.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Feed;

public static class FeedProcess
{
    public static Task<Result<FeedProcessSummary>> GetProcess(
        DailyFeedUpdater dailyFeedUpdater,
        TvUserActiveSubscriptions tvUserActiveSubscriptions,
        INewReleaseProvider releaseProvider,
        IDomainPostman domainPostman,
        CancellationToken cancellationToken = default)
    {
        // 1. Go to subs please and get the latest version of the feed
        return releaseProvider.Get()
            .InitializeProcess()
            // 2. Store the feed in storage TODO: remove if is not used anywhere
            .StoreLatestFeed(dailyFeedUpdater, cancellationToken)
            // 3. Add users to notify 
            .AddUsersToNotify(tvUserActiveSubscriptions, cancellationToken)
            // 4. Aggregate notifications
            .AggregateNotifications()
            // 5. Sent them!;
            .SendMessages(domainPostman, cancellationToken);
    }


    private static Task<Result<FeedProcessData>> InitializeProcess(this Task<Result<DailySeriesFeed[]>> processData)
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
        private Task<Result<FeedProcessData>> StoreLatestFeed(DailyFeedUpdater updater, CancellationToken token)
        {
            return data.Bind(d => updater(d.DailyFeed, token).Map(_ => d));
        }

        private Task<Result<FeedProcessData>> AddUsersToNotify(TvUserActiveSubscriptions getter,
            CancellationToken token)
        {
            return data.Bind(d => getter(d.DailyFeed.Select(x => x.Title), token)
                .Map(subscriptions => d with {Subscriptions = subscriptions}));
        }

        private Task<Result<FeedNotification[]>> AggregateNotifications()
        {
            return data.Map(d => d.Subscriptions.Select(user =>
                {
                    var userTitles = new HashSet<string>(user.Subscriptions.Select(s => s.SeriesFeedTitle));
                    var relevantFeeds = d.DailyFeed.Where(feed => userTitles.Contains(feed.Title)).ToArray();
                    return new {User = user, Feeds = relevantFeeds};
                })
                .Where(pair => pair.Feeds.Length > 0)
                .Select(pair => new FeedNotification(pair.User, pair.Feeds))
                .ToArray());
        }
    }

 
}