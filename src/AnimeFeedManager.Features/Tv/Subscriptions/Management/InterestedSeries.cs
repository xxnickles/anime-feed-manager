using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class InterestedSeries
{
    public static Task<Result<SubscriptionStorage>> VerifyStorage(
        AuthenticatedUser user,
        string seriesId,
        string seriesTitle,
        TvSubscriptionGetter subscriptionGetterGetter,
        CancellationToken token) => subscriptionGetterGetter(user.UserId, seriesId, token)
        .WithOperationName(nameof(VerifyStorage))
        .WithLogProperties([
            new KeyValuePair<string, object>("UserId", user.UserId),
            new KeyValuePair<string, object>("SeriesId", seriesId),
            new KeyValuePair<string, object>("SeriesTitle", seriesTitle)
        ])
        .Bind(subscription => VerifyCurrentSubscription(subscription, user.UserId, seriesId, seriesTitle, user.Email));


    // Toggles the stored state and returns the resulting one.
    public static Task<Result<SubscriptionType>> UpdateInterested(this Task<Result<SubscriptionStorage>> storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) =>
        storage.Bind(s => ToggleSubscription(s, subscriptionUpdater, subscriptionsRemover, token));

    private static Result<SubscriptionStorage> VerifyCurrentSubscription(
        SubscriptionStorage? subscription,
        string userId,
        string seriesId,
        string seriesTitle,
        string userEmail)
    {
        if (subscription is null)
            return new SubscriptionStorage
            {
                PartitionKey = userId,
                RowKey = seriesId,
                Type = nameof(SubscriptionType.None),
                SeriesTitle = seriesTitle,
                UserEmail = userEmail
            };

        if (subscription.Type == nameof(SubscriptionType.Subscribed))
            return Error.Create($"You are already subscribed to {seriesTitle}");

        return subscription;
    }


    private static Task<Result<SubscriptionType>> ToggleSubscription(
        SubscriptionStorage storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Type switch
    {
        nameof(SubscriptionType.None) => subscriptionUpdater(AddInterestedValue(storage), token)
            .Map(_ => SubscriptionType.Interested),
        nameof(SubscriptionType.Interested) => subscriptionsRemover(storage.PartitionKey ?? string.Empty,
                storage.RowKey ?? string.Empty, token)
            .Map(_ => SubscriptionType.None),
        _ => throw new ArgumentOutOfRangeException() // SubscriptionType.Subscribed should not be possible here
    };


    private static SubscriptionStorage AddInterestedValue(SubscriptionStorage storage)
    {
        storage.Type = nameof(SubscriptionType.Interested);
        return storage;
    }
}