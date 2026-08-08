using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Subscriptions.Entities;
using AnimeFeedManager.Features.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Subscriptions;

/// <summary>
/// Subscribe/unsubscribe: writes the users-side <see cref="UserSubscription"/> first (the source
/// of truth for "am I subscribed"), then the feeds-side <see cref="SeriesSubscriber"/> (the
/// notification-dispatch reverse index) and a <see cref="SubscriptionEvent"/> (the observability
/// stream). Both of the latter are best-effort — a failure is logged but doesn't fail the
/// operation; drift between them and the source of truth is rare and low-severity at this app's
/// scale, so there's no compensation/rollback on the first write.
/// </summary>
internal static class SeriesSubscriptions
{
    public static Task<Result<Unit>> Subscribe(
        string userId,
        int seriesId,
        SeriesSeason season,
        UserSubscriptionUpserter upsertUserSubscription,
        SeriesSubscriberUpserter upsertSeriesSubscriber,
        SubscriptionEventUpserter upsertSubscriptionEvent,
        TimeProvider time,
        CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var subscription = new UserSubscription(userId, seriesId) { Season = season, SubscribedAt = now };

        return upsertUserSubscription(subscription, cancellationToken)
            .Bind(_ => upsertSeriesSubscriber(new SeriesSubscriber(seriesId, userId), cancellationToken)
                .AddLogOnFailure(_ => log => log.LogWarning(
                    "Failed to write feeds-side subscriber index for series {SeriesId}, user {UserId}", seriesId, userId))
                .BindOnErrorWhen(binder: _ => new Unit(), predicate: _ => true))
            .Bind(_ => upsertSubscriptionEvent(
                    new SubscriptionEvent(SubscriptionSources.SubscriptionActivity)
                    {
                        UserId = userId,
                        SeriesId = seriesId,
                        Kind = SubscriptionEventKind.Created,
                        OccurredAt = now
                    }, cancellationToken)
                .AddLogOnFailure(_ => log => log.LogWarning(
                    "Failed to write subscription event for series {SeriesId}, user {UserId}", seriesId, userId))
                .BindOnErrorWhen(binder: _ => new Unit(), predicate: _ => true));
    }

    public static Task<Result<Unit>> Unsubscribe(
        string userId,
        int seriesId,
        UserSubscriptionRemover removeUserSubscription,
        SeriesSubscriberRemover removeSeriesSubscriber,
        SubscriptionEventUpserter upsertSubscriptionEvent,
        TimeProvider time,
        CancellationToken cancellationToken) =>
        removeUserSubscription(userId, seriesId, cancellationToken)
            .Bind(_ => removeSeriesSubscriber(seriesId, userId, cancellationToken)
                .AddLogOnFailure(_ => log => log.LogWarning(
                    "Failed to remove feeds-side subscriber index for series {SeriesId}, user {UserId}", seriesId, userId))
                .BindOnErrorWhen(binder: _ => new Unit(), predicate: _ => true))
            .Bind(_ => upsertSubscriptionEvent(
                    new SubscriptionEvent(SubscriptionSources.SubscriptionActivity)
                    {
                        UserId = userId,
                        SeriesId = seriesId,
                        Kind = SubscriptionEventKind.Removed,
                        OccurredAt = time.GetUtcNow()
                    }, cancellationToken)
                .AddLogOnFailure(_ => log => log.LogWarning(
                    "Failed to write subscription event for series {SeriesId}, user {UserId}", seriesId, userId))
                .BindOnErrorWhen(binder: _ => new Unit(), predicate: _ => true));
}
