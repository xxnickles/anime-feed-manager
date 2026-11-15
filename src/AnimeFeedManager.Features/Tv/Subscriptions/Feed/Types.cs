using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed;

public record FeedProcessSummary(int UsersToNotify);

public record UserNotificationResult(string UserId, NotificationSent[] Notifications);

public record UserNotificationSummary(string UserId, int NotificationsSent);

