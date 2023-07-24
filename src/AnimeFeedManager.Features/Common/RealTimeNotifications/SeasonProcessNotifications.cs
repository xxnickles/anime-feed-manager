namespace AnimeFeedManager.Features.Common.RealTimeNotifications;

public enum TargetAudience{
    All,
    Admins
}

public enum NotificationType
{
    Information,
    Update,
    Error
}

public abstract record RealtimeNotification(string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message);

public record SeasonProcessNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    SimpleSeasonInfo Season,
    SeriesType SeriesType,
    string Message
) : RealtimeNotification(Id, TargetAudience, Result, Message);

public record TitlesUpdateNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message
) : RealtimeNotification(Id, TargetAudience, Result, Message);

public record ImageUpdateNotification(string Id,NotificationType Result, SeriesType SeriesType, string Message) : RealtimeNotification(Id,
    TargetAudience.Admins, Result, Message);