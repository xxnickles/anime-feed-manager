namespace AnimeFeedManager.Features.Domain.Notifications
{
    public record SeasonProcessNotification(
        string Id,
        TargetAudience TargetAudience,
        NotificationType Result,
        SeasonInfoDto Season,
        SeriesType SeriesType,
        string Message
    ) : Notification(Id, TargetAudience, Result, Message);
}