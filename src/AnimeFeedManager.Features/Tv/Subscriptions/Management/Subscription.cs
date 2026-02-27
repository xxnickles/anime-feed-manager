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
        .Map(subscriptions => subscriptions.Select(MarkAsExpired).ToImmutableArray())
        .Bind(subscriptions => StoreUpdates(subscriptions, subscriptionUpdater, token));


    private static async Task<Result<Summary>> StoreUpdates(
        ImmutableArray<SubscriptionStorage> subscriptions,
        TvSubscriptionUpdater subscriptionUpdater,
        CancellationToken token)
    {
        var results = await Task.WhenAll(
            subscriptions.Select(s =>
                subscriptionUpdater(s, token)
                    .WithOperationName(nameof(StoreUpdates))
                    .WithLogProperty("Subscription", s)));

        return results
            .Flatten(units => units.Count())
            .AddLogOnSuccess(LogFactories.LogBulkResult<int>(
                (count, logger) => logger.LogInformation("{Count} subscriptions expired", count)))
            .Map(bulk => new Summary(bulk.Value));
    }


    private static SubscriptionStorage MarkAsExpired(SubscriptionStorage storage)
    {
        storage.Status = nameof(SubscriptionStatus.Expired);
        return storage;
    }

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
                Status = nameof(SubscriptionStatus.Active),
                SeriesFeedTitle = feedTitle,
                SeriesTitle = seriesTitle,
                UserEmail = user.Email,
                SeriesLink = seriesLink
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