using AnimeFeedManager.Features.Common.Domain.Notifications;
using AnimeFeedManager.Features.Common.Domain.Notifications.Base;

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
    Tv,
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

