using AnimeFeedManager.Common.Notifications.Realtime;

namespace AnimeFeedManager.WebApp.Services;

public enum HubConnectionStatus
{
    None,
    Connected,
    Disconnected
}

public enum NotificationSource
{
    None,
    TV,
    Ovas,
    Movies,
    Titles
}

public record ServerNotification(
    string Id,
    bool Read,
    DateTime Time,
    NotificationType Type,
    NotificationSource Source,
    TargetAudience Audience,
    string Message);

