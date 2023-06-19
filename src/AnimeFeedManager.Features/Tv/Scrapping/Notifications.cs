using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Dto;
using AnimeFeedManager.Features.Infrastructure.Notifications;

namespace AnimeFeedManager.Features.Tv.Scrapping;

public record SeasonProcessNotification(
    string Id,
    TargetAudience TargetAudience,
    NotificationType Result,
    SeasonInfoDto Season,
    SeriesType SeriesType,
    string Message
) : RealtimeNotification(Id, TargetAudience, Result, Message);