using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class InterestedSeries
{
    public static Task<Result<SubscriptionStorage>> VerifyStorage(
        string userId,
        string seriesId,
        string seriesTitle,
        TvSubscriptionGetter subscriptionGetterGetter,
        CancellationToken token) => subscriptionGetterGetter(userId, seriesId, token)
        .Bind(subscription => VerifyCurrentSubscription(subscription, userId, seriesId, seriesTitle));


    public static Task<Result<Unit>> UpdateInterested(this Task<Result<SubscriptionStorage>> storage,  
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Bind(s => ToggleSubscription(s, subscriptionUpdater, subscriptionsRemover, token));
    
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
                Type = nameof(SubscriptionType.None),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = seriesTitle,
            };

        if (subscription.Type == nameof(SubscriptionType.Subscribed))
            return Error.Create($"You are already subscribed to {seriesTitle}")
                .WithLogProperty("UserId", userId)
                .WithLogProperty("SeriesId", seriesId)
                .WithOperationName(nameof(VerifyCurrentSubscription));

        return subscription;
    }


    private static Task<Result<Unit>> ToggleSubscription(
        SubscriptionStorage storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Type switch
    {
        nameof(SubscriptionType.None) => subscriptionUpdater(AddInterestedValue(storage), token),
        nameof(SubscriptionType.Interested) => subscriptionsRemover(storage.PartitionKey ?? string.Empty,
            storage.RowKey ?? string.Empty, token),
        _ => throw new ArgumentOutOfRangeException() // SubscriptionType.Subscribed should not be possible here
    };


    private static SubscriptionStorage AddInterestedValue(SubscriptionStorage storage)
    {
        storage.Type = nameof(SubscriptionType.Interested);
        return storage;
    }
}