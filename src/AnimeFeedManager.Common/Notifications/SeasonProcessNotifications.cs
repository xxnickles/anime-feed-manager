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

public enum SeriesType
{
    Tv,
    Ova,
    Movie
}

public abstract record RealtimeNotification(string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    string Message);

public record SeasonProcessNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    SeasonInfoDto Season,
    SeriesType SeriesType,
    string Message
):RealtimeNotification(Id, TargetAudience, Result, Message);

public record TitlesUpdateNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    SeriesType SeriesType,
    string Message
):RealtimeNotification(Id, TargetAudience, Result, Message);