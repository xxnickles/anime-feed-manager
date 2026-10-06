using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class Subscription
{
    public static Task<Result<SubscriptionStorage>> VerifyStorage(
        AuthenticatedUser user,
        string seriesId,
        string seriesTitle,
        string feedTitle,
        string seriesLink,
        TvSubscriptionGetter subscriptionGetterGetter,
        CancellationToken token) =>
        subscriptionGetterGetter(user.UserId, seriesId, token)
            .WithOperationName(nameof(VerifyStorage))
            .WithLogProperties([
                new KeyValuePair<string, object>(nameof(seriesId), seriesId),
                new KeyValuePair<string, object>(nameof(user), user.UserId)
            ])
            .Map(subscription =>
                VerifyCurrentSubscription(subscription, user, seriesId, seriesTitle, feedTitle, seriesLink));

    // Toggles the stored state and returns the resulting one.
    public static Task<Result<SubscriptionType>> UpdateSubscription(this Task<Result<SubscriptionStorage>> storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) =>
        storage.Bind(s => ToggleSubscription(s, subscriptionUpdater, subscriptionsRemover, token));

    private static SubscriptionStorage VerifyCurrentSubscription(
        SubscriptionStorage? subscription,
        AuthenticatedUser user,
        string seriesId,
        string seriesTitle,
        string feedTitle,
        string seriesLink)
    {
        if (subscription is null)
            return new SubscriptionStorage
            {
                PartitionKey = user.UserId,
                RowKey = seriesId,
                Type = nameof(SubscriptionType.None),
                SeriesFeedTitle = feedTitle,
                SeriesTitle = seriesTitle,
                UserEmail = user.Email,
                SeriesLink = seriesLink
            };

        return subscription;
    }

    private static Task<Result<SubscriptionType>> ToggleSubscription(
        SubscriptionStorage storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) => storage.Type switch
    {
        nameof(SubscriptionType.None) => subscriptionUpdater(AddSubscribedValue(storage), token)
            .Map(_ => SubscriptionType.Subscribed),
        nameof(SubscriptionType.Subscribed) => subscriptionsRemover(storage.PartitionKey ?? string.Empty,
                storage.RowKey ?? string.Empty, token)
            .Map(_ => SubscriptionType.None),
        _ => throw new ArgumentOutOfRangeException() // SubscriptionType.Interested should not be possible here
    };

    private static SubscriptionStorage AddSubscribedValue(SubscriptionStorage storage)
    {
        storage.Type = nameof(SubscriptionType.Subscribed);
        return storage;
    }
}