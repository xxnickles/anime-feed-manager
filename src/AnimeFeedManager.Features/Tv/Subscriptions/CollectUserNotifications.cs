using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Feed.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public class CollectUserNotifications
{
    private readonly IFeedProvider _feedProvider;
    private readonly IGetTvSubscriptions _subscriptionsGetter;
    private readonly IGetProcessedTitles _processedTitlesGetter;
    private readonly IDomainPostman _domainPostman;

    public CollectUserNotifications(
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

    public Task<Either<DomainError, SubscriberNotification>> Get(UserId userId, CancellationToken token)
    {
        _subscriptionsGetter.GetUserSubscriptions(userId, token)
            .BindAsync(collection => FilterProcessed(userId, collection, token))
            .BindAsync(collection =>  )
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

    private Task<Either<DomainError, SubscriberNotification>> GetNotifications(SubscriptionCollection collection)
    {
        _feedProvider.GetFeed(Resolution.Hd)
            .MapAsync(feed => feed.Where(item => collection.Series.Contains(item.AnimeTitle)))
    }
    
    private 
}