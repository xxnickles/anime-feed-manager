using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Feed.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public class UserNotificationsCollector
{
    private readonly IFeedProvider _feedProvider;
    private readonly IGetTvSubscriptions _subscriptionsGetter;
    private readonly IGetProcessedTitles _processedTitlesGetter;
    private readonly IDomainPostman _domainPostman;

    public UserNotificationsCollector(
        IFeedProvider feedProvider,
        IGetTvSubscriptions subscriptionsGetter,
        IGetProcessedTitles processedTitlesGetter,
        IDomainPostman domainPostman)
    {
        _feedProvider = feedProvider;
        _subscriptionsGetter = subscriptionsGetter;
        _processedTitlesGetter = processedTitlesGetter;
        _domainPostman = domainPostman;
    }

    public Task<Either<DomainError, CollectedNotificationResult>> Get(UserId userId, CancellationToken token = default)
    {
        return _subscriptionsGetter.GetUserSubscriptions(userId, token)
            .BindAsync(collection => FilterProcessed(userId, collection, token))
            .BindAsync(collection => GetNotifications(userId, collection))
            .BindAsync(notification => CreateNotificationEvent(notification, token));
    }


    private Task<Either<DomainError, SubscriptionCollection>> FilterProcessed(UserId userId,
        SubscriptionCollection collection,
        CancellationToken token)
    {
        return _processedTitlesGetter.GetForUser(userId, token)
            .MapAsync(titles => collection with
            {
                Series = collection.Series.Where(title => !titles.Contains(title)).ToImmutableList()
            });
    }

    private Either<DomainError, SubscriberTvNotification> GetNotifications(UserId userId,
        SubscriptionCollection collection)
    {
        return _feedProvider.GetFeed(Resolution.Hd)
            .Map(feed => CreateNotification(userId, collection, feed));
    }

    private static SubscriberTvNotification CreateNotification(UserId userId, SubscriptionCollection subscription,
        IEnumerable<FeedInfo> feed)
    {
        var matchingFeeds = feed
            .Where(feedInfo => subscription.Series.Select(x => x.ToString()).Contains(feedInfo.AnimeTitle))
            .Select(
                x => new SubscribedFeed(
                    x.AnimeTitle,
                    x.Links.ToArray(),
                    x.EpisodeInfo,
                    x.PublicationDate));

        return new SubscriberTvNotification(userId, matchingFeeds.ToArray());
    }

    private Task<Either<DomainError, CollectedNotificationResult>> CreateNotificationEvent(
        SubscriberTvNotification tvNotification, CancellationToken token)
    {
        if (tvNotification.Feeds.Length < 1)
        {
            return Task.FromResult<Either<DomainError, CollectedNotificationResult>>(
                NoContentError.Create($"No subscriptions have been found for {tvNotification.Subscriber} "));
        }


        return _domainPostman.SendMessage(tvNotification, Box.TvNotifications, token)
            .MapAsync(_ =>
                new CollectedNotificationResult((byte) tvNotification.Feeds.Length, tvNotification.Subscriber));
    }
}