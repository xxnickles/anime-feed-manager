namespace AnimeFeedManager.Features.Domain.Notifications;

public record SeasonProcessNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    SimpleSeasonInfo SimpleSeason,
    SeriesType SeriesType,
    string Message
) : Notification(Id, TargetAudience, Result, Message);

public record ImageUpdateNotification(string Id,NotificationType Result, SeriesType SeriesType, string Message) : Notification(Id,
    TargetAudience.Admins, Result, Message);