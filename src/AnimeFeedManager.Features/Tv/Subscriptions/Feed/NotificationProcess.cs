using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed;

public static class NotificationProcess
{
    public static Task<Result<UserNotificationSummary>> UpdateUserSubscriptions(
        FeedNotification updateNotification,
        TvSubscriptionsUpdater updater,
        IDomainPostman domainPostman,
        CancellationToken token)
    {
        return UpdateUserSubscription(updateNotification, updater, token)
            .Map(_ => ToUserNotificationResult(updateNotification))
            .Bind(results => SendNotifications(domainPostman, results))
            .Summarize();
    }

    private static Task<Result<Unit>> UpdateUserSubscription(
        FeedNotification updateNotification,
        TvSubscriptionsUpdater updater,
        CancellationToken token)
    {
        // Build a lookup from feed title -> feed for fast matching
        var feedByTitle = updateNotification.Feeds
            .ToDictionary(f => f.Title, f => f);

        var storagesToUpdate = updateNotification.Subscriptions.Subscriptions
            .Select(s =>
            {
                // Match the daily feed title with the active subscription SeriesFeedTitle
                if (!feedByTitle.TryGetValue(s.SeriesFeedTitle, out var feed))
                    return null;

                // Aggregate: already-notified episodes + newly-notified episodes from the feed notification
                var aggregatedEpisodes = s.NotifiedEpisodes
                    .Concat(feed.Episodes.Select(e => e.EpisodeNumber))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();

                // Use the same title for SeriesTitle and SeriesFeedTitle (per your requirement)
                return new SubscriptionStorage
                {
                    PartitionKey = updateNotification.Subscriptions.UserId,
                    RowKey = s.SeriesId,
                    Type = nameof(SubscriptionType.Subscribed),
                    Status = nameof(SubscriptionStatus.Active),
                    SeriesTitle = feed.Title,
                    SeriesFeedTitle = feed.Title,
                    SeriesLink = feed.Url,
                    NotifiedEpisodes = aggregatedEpisodes.AppArrayToString(),
                    UserEmail = updateNotification.Subscriptions.UserEmail
                };
            })
            .Where(x => x is not null)
            .Cast<SubscriptionStorage>();

        return updater(storagesToUpdate, token).MapError(error => error
            .WithOperationName(nameof(UpdateUserSubscription)));
    }

    private static UserNotificationResult ToUserNotificationResult(FeedNotification updateNotification)
    {
        return new UserNotificationResult(
            UserId: updateNotification.Subscriptions.UserId,
            Notifications: updateNotification.Feeds
                .Select(f => new NotificationSent(
                    Title: f.Title,
                    Url: f.Url,
                    Episodes: f.Episodes
                        .Select(e => e.EpisodeNumber)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray()
                ))
                .ToArray()
        );
    }

    private static Task<Result<UserNotificationResult>> SendNotifications(IDomainPostman domainPostman, UserNotificationResult result)
    {
        var messages = result.Notifications.Select(n =>
            new SystemEvent(
                TargetConsumer.User(result.UserId),
                EventTarget.LocalStorage,
                EventType.Completed,
                n.AsEventPayload()));

        return domainPostman.SendMessages(messages)
            .Map(_ => result)
            .MapError(error => error.WithOperationName(nameof(SendNotifications)));
    }
    
    private static Task<Result<UserNotificationSummary>> Summarize(this Task<Result<UserNotificationResult>> result) =>
        result.Map(r => new UserNotificationSummary(r.UserId, r.Notifications.Length));
}