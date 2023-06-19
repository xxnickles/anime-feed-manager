namespace AnimeFeedManager.Features.Infrastructure.Notifications;

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
    NotificationType Result,
    string Message);