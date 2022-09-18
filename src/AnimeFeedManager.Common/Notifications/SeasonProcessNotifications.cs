using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Common.Notifications;

public enum TargetAudience
{
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
    NotificationType Result);

public record SeasonProcessNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    SeasonInfoDto Season,
    string Message
):RealtimeNotification(Id, TargetAudience, Result);

public record TitlesUpdateNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message
):RealtimeNotification(Id, TargetAudience, Result);