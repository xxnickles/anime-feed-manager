using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Common.Notifications;

public readonly record struct ShortSeries(string Title, DateTime Publication);
public readonly record struct NotificationResult(string Id, int Completed, int Errors, bool ShouldNotify);

public abstract record Notification;
public  record  StringNotification(string Value): Notification;
public  record  TvNotification(DateTime Time, IEnumerable<SubscribedFeed> Feeds) : Notification;

public  record  ShortSeriesNotification(DateTime Time, IEnumerable<ShortSeries> Feeds): Notification;
public  record  UpdateNotification(int Completed, int Errors): Notification;

