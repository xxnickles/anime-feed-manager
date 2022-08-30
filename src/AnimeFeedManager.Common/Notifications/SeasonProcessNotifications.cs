namespace AnimeFeedManager.Common.Notifications;

public enum TargetAudience
{
    All,
    Admins
}

public enum NotificationType
{
    Information,
    Error
}

public record SeasonProcessNotification(
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message
);