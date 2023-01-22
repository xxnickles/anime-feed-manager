using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Common.Notifications;

public readonly record struct NotificationResult(string Id, int Completed, int Errors, bool ShouldNotify);
public readonly record struct TvNotification(DateTime Time, IEnumerable<SubscribedFeed> Feeds);
public readonly record struct UpdateNotification(int Completed, int Errors);
