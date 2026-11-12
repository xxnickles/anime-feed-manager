using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Management;

public static class Subscription
{
    public static Task<Result<SubscriptionStorage, DomainError>> VerifyStorage(
        AuthenticatedUser user,
        string seriesId,
        string seriesTitle,
        string feedTitle,
        TvSubscriptionGetter subscriptionGetterGetter,
        CancellationToken token) =>
        subscriptionGetterGetter(user.UserId, seriesId, token)
            .WithOperationName(nameof(VerifyStorage))
            .WithLogProperties([
                new KeyValuePair<string, object>(Telemetry.SeriesIdTag, seriesId),
                new KeyValuePair<string, object>(Telemetry.UserIdTag, user.UserId)
            ])
            .Bind(subscription =>
                VerifyCurrentSubscription(subscription, user, seriesId, seriesTitle, feedTitle));

    // Toggles the stored state and returns the resulting one.
    public static Task<Result<SubscriptionType, DomainError>> UpdateSubscription(this Task<Result<SubscriptionStorage, DomainError>> storage,
        TvSubscriptionUpdater subscriptionUpdater,
        TvSubscriptionsRemover subscriptionsRemover,
        CancellationToken token) =>
        storage.Bind(s => ToggleSubscription(s, subscriptionUpdater, subscriptionsRemover, token));

    // The feed title keys notifications, so a new subscription needs one; a stored one stays removable.
    private static Result<SubscriptionStorage, DomainError> VerifyCurrentSubscription(
        SubscriptionStorage? subscription,
        AuthenticatedUser user,
        string seriesId,
        string seriesTitle,
        string feedTitle) => subscription switch
    {
        not null => subscription,
        _ when string.IsNullOrWhiteSpace(feedTitle) => Error.Create($"{seriesTitle} has no feed to subscribe to"),
        _ => new SubscriptionStorage
        {
            PartitionKey = user.UserId,
            RowKey = seriesId,
            Type = nameof(SubscriptionType.None),
            SeriesFeedTitle = feedTitle,
            SeriesTitle = seriesTitle,
            UserEmail = user.Email
        }
    };

    private static Task<Result<SubscriptionType, DomainError>> ToggleSubscription(
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