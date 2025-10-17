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
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) =>
        storage.Bind(s => ToggleSubscription(s, subscriptionUpdater, subscriptionsRemover, token));


    public static Task<Result<Summary>> ExpireSubscriptions(
        string seriesId,
        TvSubscriptionsBySeries getter,
        TvSubscriptionUpdater subscriptionUpdater,
        CancellationToken token) => getter(seriesId, token)
        .Map(subscriptions => subscriptions.ConvertAll(MarkAsExpired))
        .Bind(subscriptions => StoreUpdates(subscriptions, subscriptionUpdater, token));


    private static Task<Result<Summary>> StoreUpdates(
        ImmutableList<SubscriptionStorage> subscriptions,
        TvSubscriptionUpdater subscriptionUpdater,
        CancellationToken token)
    {
       return subscriptions.Select(s =>
            subscriptionUpdater(s, token)
                .MapError(error => error
                    .WithLogProperty("Subscription", s)
                    .WithOperationName(nameof(StoreUpdates))
                ))
           .Flatten()
           .Map(r => new Summary(r.Count));
    }


    private static SubscriptionStorage MarkAsExpired(SubscriptionStorage storage)
    {
        storage.Status = nameof(SubscriptionStatus.Expired);
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
                Type = nameof(SubscriptionType.None),
                Status = nameof(SubscriptionStatus.Active),
                SeriesFeedTitle = feedTitle,
                SeriesTitle = seriesTitle,
            };

        return subscription;
    }

    private static Task<Result<Unit>> ToggleSubscription(
        SubscriptionStorage storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Type switch
    {
        nameof(SubscriptionType.None) => subscriptionUpdater(AddSubscribedValue(storage), token),
        nameof(SubscriptionType.Subscribed) => subscriptionsRemover(storage.PartitionKey ?? string.Empty,
            storage.RowKey ?? string.Empty, token),
        _ => throw new ArgumentOutOfRangeException() // SubscriptionType.Interested should not be possible here
    };

    private static SubscriptionStorage AddSubscribedValue(SubscriptionStorage storage)
    {
        storage.Type = nameof(SubscriptionType.Subscribed);
        return storage;
    }
}