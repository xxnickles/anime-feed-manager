using AnimeFeedManager.Features.Subscriptions.Entities;

namespace AnimeFeedManager.Features.Subscriptions.Storage;

/// <summary>Subscribes a user to a series — plain create, idempotent (re-subscribing just re-upserts).</summary>
public delegate Task<Result<Unit>> UserSubscriptionUpserter(
    UserSubscription subscription, CancellationToken cancellationToken);

/// <summary>Unsubscribes a user from a series — plain delete; already-unsubscribed is a no-op success.</summary>
public delegate Task<Result<Unit>> UserSubscriptionRemover(
    string userId, int seriesId, CancellationToken cancellationToken);

/// <summary>Every series a user subscribes to — single-partition query, no cross-partition fan-out.</summary>
public delegate Task<Result<ImmutableArray<UserSubscription>>> UserSubscriptionsLoader(
    string userId, CancellationToken cancellationToken);

/// <summary>Write-only — one document per subscribe/unsubscribe occurrence.</summary>
public delegate Task<Result<Unit>> SubscriptionEventUpserter(SubscriptionEvent subscriptionEvent, CancellationToken cancellationToken);

/// <summary>Most recent <paramref name="take"/> subscription events, newest first.</summary>
public delegate Task<Result<ImmutableArray<SubscriptionEvent>>> RecentSubscriptionEventsLoader(
    int take, CancellationToken cancellationToken);
