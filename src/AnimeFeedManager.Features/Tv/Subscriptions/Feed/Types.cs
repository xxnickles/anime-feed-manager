using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tv.Subscriptions.Feed;

public record FeedProcessData(DailySeriesFeed[] DailyFeed, UserActiveSubscriptions[] Subscriptions);

public record FeedProcessSummary(int UsersToNotify);

public record UserNotificationResult(string UserId, NotificationSent[] Notifications);

public record UserNotificationSummary(string UserId, int NotificationsSent);

