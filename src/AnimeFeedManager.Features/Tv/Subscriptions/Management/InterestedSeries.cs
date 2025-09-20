using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class InterestedSeries
{
    public static Task<Result<SubscriptionStorage>> VerifyStorage(
        string userId,
        string seriesId,
        string seriesTitle,
        StorageTvSubscription subscriptionGetter,
        CancellationToken token) => subscriptionGetter(userId, seriesId, token)
        .Bind(subscription => VerifyCurrentSubscription(subscription, userId, seriesId, seriesTitle));


    public static Task<Result<Unit>> UpdateInterested(this Task<Result<SubscriptionStorage>> storage,  
        TvSubscriptionsUpdater subscriptionsUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Bind(s => ToggleSubscription(s, subscriptionsUpdater, subscriptionsRemover, token));
    
    private static Result<SubscriptionStorage> VerifyCurrentSubscription(
        SubscriptionStorage? subscription,
        string userId,
        string seriesId,
        string seriesTitle)
    {
        if (subscription is null)
            return new SubscriptionStorage
            {
                PartitionKey = userId,
                RowKey = seriesId,
                Type = SubscriptionType.None,
                Status = SubscriptionStatus.Active,
                SeriesTitle = seriesTitle,
            };

        if (subscription.Type == SubscriptionType.Subscribed)
            return Error.Create("")
                .WithLogProperty("User", userId)
                .WithLogProperty("Series", seriesId)
                .WithLogProperty("Operation", nameof(VerifyCurrentSubscription));

        return subscription;
    }


    private static Task<Result<Unit>> ToggleSubscription(
        SubscriptionStorage storage,
        TvSubscriptionsUpdater subscriptionsUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Type switch
    {
        SubscriptionType.None => subscriptionsUpdater(AddInterestedValue(storage), token),
        SubscriptionType.Interested => subscriptionsRemover(storage.PartitionKey ?? string.Empty,
            storage.RowKey ?? string.Empty, token),
        _ => throw new ArgumentOutOfRangeException() // SubscriptionType.Subscribed should not be possible here
    };


    private static SubscriptionStorage AddInterestedValue(SubscriptionStorage storage)
    {
        storage.Type = SubscriptionType.Interested;
        return storage;
    }
}