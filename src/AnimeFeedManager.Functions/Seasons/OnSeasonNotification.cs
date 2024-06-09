using System.Diagnostics;
using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Seasons;

public sealed class OnSeasonNotification(
    IStoreNotification storeNotification,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnSeasonNotification> _logger = loggerFactory.CreateLogger<OnSeasonNotification>();

    [Function("OnSeasonNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(SeasonProcessNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        SeasonProcessNotification notification, CancellationToken token)
    {
        // Stores notification and create event to update latest seasons
        var result = await storeNotification.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            FromNotification(notification.SeriesType),
            NotificationArea.Update,
            notification,
            token);


        return result.Match(
            _ => CreateMessage(notification),
            e =>
            {
                e.LogError(_logger);
                return CreateMessage(notification);
            }
        );
    }

    private static SignalRMessageAction CreateMessage(SeasonProcessNotification notification)
    {
        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments =
            [
                notification
            ]
        };
    }

    private static NotificationTarget FromNotification(SeriesType type) => type switch
    {
        SeriesType.Tv => NotificationTarget.Tv,
        SeriesType.Movie => NotificationTarget.Movie,
        SeriesType.Ova => NotificationTarget.Ova,
        SeriesType.None => NotificationTarget.None,
        _ => throw new UnreachableException("Value not expected")
    };
}