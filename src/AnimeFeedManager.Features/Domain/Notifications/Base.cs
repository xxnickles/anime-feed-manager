namespace AnimeFeedManager.Features.Domain.Notifications;

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

public abstract record Notification(string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message);