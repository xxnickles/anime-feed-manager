using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Common.Notifications;

public record struct NotificationResult(int Completed, int Errors, bool IsCompleted);
public record struct TvNotification(DateTime Time, IEnumerable<SubscribedFeed> Feeds);
public record struct UpdateNotification(int Completed, int Errors);
