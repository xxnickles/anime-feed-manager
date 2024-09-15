using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Feed.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public class UserNotificationsCollector(
    IFeedProvider feedProvider,
    IGetTvSubscriptions subscriptionsGetter,
    IGetProcessedTitles processedTitlesGetter,
    IDomainPostman domainPostman)
{
    public Task<Either<DomainError, CollectedNotificationResult>> Get(UserId userId, CancellationToken token = default)
    {
        return subscriptionsGetter.GetUserSubscriptions(userId, token)
            .BindAsync(collection => FilterProcessed(userId, collection, token))
            .BindAsync(collection => GetNotifications(userId, collection))
            .BindAsync(notification => CreateNotificationEvent(notification, token));
    }


    private Task<Either<DomainError, SubscriptionCollection>> FilterProcessed(UserId userId,
        SubscriptionCollection collection,
        CancellationToken token)
    {
        return processedTitlesGetter.GetForUser(userId, token)
            .MapAsync(titles => collection with
            {
                Series = collection.Series.Where(title => !titles.Contains(title)).ToImmutableList()
            });
    }

    private Either<DomainError, SubscriberTvNotification> GetNotifications(UserId userId,
        SubscriptionCollection collection)
    {
        return feedProvider.GetFeed(Resolution.Hd)
            .Map(feed => CreateNotification(userId, collection, feed));
    }

    private static SubscriberTvNotification CreateNotification(UserId userId, SubscriptionCollection subscription,
        IEnumerable<FeedInfo> feed)
    {
        var subscribedSeries = subscription.Series.Select(x => x.ToString());
        var matchingFeeds = feed
            .Where(feedInfo => subscribedSeries.Contains(feedInfo.AnimeTitle))
            .Select(
                x => new SubscribedFeed(
                    x.AnimeTitle,
                    x.Links.ToArray(),
                    x.EpisodeInfo,
                    x.PublicationDate));

        return new SubscriberTvNotification(subscription.SubscriberEmail, userId, matchingFeeds.ToArray());
    }

    private Task<Either<DomainError, CollectedNotificationResult>> CreateNotificationEvent(
        SubscriberTvNotification tvNotification, CancellationToken token)
    {
        if (tvNotification.Feeds.Length < 1)
        {
            return Task.FromResult<Either<DomainError, CollectedNotificationResult>>(
                NoContentError.Create($"No subscriptions have been found for {tvNotification.Subscriber} "));
        }


        return domainPostman.SendMessage(tvNotification, token)
            .MapAsync(_ =>
                new CollectedNotificationResult((byte) tvNotification.Feeds.Length, tvNotification.Subscriber));
    }
}