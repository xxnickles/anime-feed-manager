using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class Subscription
{
    public static Task<Result<SubscriptionStorage>> VerifyStorage(
        string userId,
        string seriesId,
        string seriesTitle,
        string feedTitle,
        TvSubscriptionGetter subscriptionGetterGetter,
        CancellationToken token) => subscriptionGetterGetter(userId, seriesId, token)
        .Map(subscription => VerifyCurrentSubscription(subscription, userId, seriesId, seriesTitle, feedTitle));

    public static Task<Result<Unit>> UpdateSubscription(this Task<Result<SubscriptionStorage>> storage,
        TvSubscriptionsUpdater subscriptionsUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) =>
        storage.Bind(s => ToggleSubscription(s, subscriptionsUpdater, subscriptionsRemover, token));


    public static Task<Result<ExpirationSummary>> ExpireSubscriptions(
        string seriesId,
        TvSubscriptionsBySeries getter,
        TvSubscriptionsUpdater subscriptionsUpdater,
        CancellationToken token) => getter(seriesId, token)
        .Map(subscriptions => subscriptions.ConvertAll(MarkAsExpired))
        .Bind(subscriptions => StoreUpdates(subscriptions, subscriptionsUpdater, token));


    private static Task<Result<ExpirationSummary>> StoreUpdates(
        ImmutableList<SubscriptionStorage> subscriptions,
        TvSubscriptionsUpdater subscriptionsUpdater,
        CancellationToken token)
    {
       return subscriptions.Select(s =>
            subscriptionsUpdater(s, token)
                .MapError(error => error
                    .WithLogProperty("Subscription", s)
                    .WithOperationName(nameof(StoreUpdates))
                ))
           .Flatten()
           .Map(r => new ExpirationSummary(r.Count));
    }


    private static SubscriptionStorage MarkAsExpired(SubscriptionStorage storage)
    {
        storage.Status = SubscriptionStatus.Expired;
        return storage;
    }

    private static SubscriptionStorage VerifyCurrentSubscription(
        SubscriptionStorage? subscription,
        string userId,
        string seriesId,
        string seriesTitle,
        string feedTitle)
    {
        if (subscription is null)
            return new SubscriptionStorage
            {
                PartitionKey = userId,
                RowKey = seriesId,
                Type = SubscriptionType.None,
                Status = SubscriptionStatus.Active,
                SeriesFeedTitle = feedTitle,
                SeriesTitle = seriesTitle,
            };

        return subscription;
    }

    private static Task<Result<Unit>> ToggleSubscription(
        SubscriptionStorage storage,
        TvSubscriptionsUpdater subscriptionsUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Type switch
    {
        SubscriptionType.None => subscriptionsUpdater(AddSubscribedValue(storage), token),
        SubscriptionType.Subscribed => subscriptionsRemover(storage.PartitionKey ?? string.Empty,
            storage.RowKey ?? string.Empty, token),
        _ => throw new ArgumentOutOfRangeException() // SubscriptionType.Interested should not be possible here
    };

    private static SubscriptionStorage AddSubscribedValue(SubscriptionStorage storage)
    {
        storage.Type = SubscriptionType.Subscribed;
        return storage;
    }
}