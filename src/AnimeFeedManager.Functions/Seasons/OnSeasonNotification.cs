using System.Diagnostics;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Types;
using AnimeFeedManager.Features.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Seasons;

public sealed class OnSeasonNotification
{
    private readonly IStoreNotification _storeNotification;
    private readonly ILogger<OnSeasonNotification> _logger;

    public OnSeasonNotification(
        IStoreNotification storeNotification,
        ILoggerFactory loggerFactory)
    {
        _storeNotification = storeNotification;
        _logger = loggerFactory.CreateLogger<OnSeasonNotification>();
    }

    [Function("OnSeasonNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(Box.Available.SeasonProcessNotificationsBox, Connection = "AzureWebJobsStorage")]
        SeasonProcessNotification notification)
    {
        // Stores notification
        var result = await _storeNotification.Add(
            IdHelpers.GetUniqueId(),
            UserRoles.Admin,
            FromNotification(notification.SeriesType),
             NotificationArea.Update,
            notification,
            default);


        return result.Match(
            _ => CreateMessage(notification),
            e =>
            {
                e.LogDomainError(_logger);
                return CreateMessage(notification);
            }
        );
    }

    private static SignalRMessageAction CreateMessage(SeasonProcessNotification notification)
    {
        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments = new object[]
            {
                notification
            }
        };
    }

    private static NotificationTarget FromNotification(SeriesType type) => type switch
    {
        SeriesType.Tv => NotificationTarget.Tv,
        SeriesType.Movie => NotificationTarget.Movie,
        SeriesType.Ova => NotificationTarget.Ova,
        SeriesType.None => NotificationTarget.None,
        _ =>  throw new UnreachableException("Value not expected")
    };
}