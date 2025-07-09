using System.Diagnostics;
using AnimeFeedManager.Old.Common;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.RealTimeNotifications;
using AnimeFeedManager.Old.Features.Notifications.IO;
using AnimeFeedManager.Old.Web.BlazorComponents;
using AnimeFeedManager.Old.Web.BlazorComponents.SignalRContent;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Seasons;

public sealed class OnSeasonNotification(
    IStoreNotification storeNotification,
    ILoggerFactory loggerFactory,
    BlazorRenderer renderer)
{
    private readonly ILogger<OnSeasonNotification> _logger = loggerFactory.CreateLogger<OnSeasonNotification>();

    [Function(nameof(OnSeasonNotification))]
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


        return await result.Match(
            _ => CreateMessage(notification),
            e =>
            {
                e.LogError(_logger);
                return CreateMessage(notification);
            }
        );
    }

    private async Task<SignalRMessageAction> CreateMessage(SeasonProcessNotification notification)
    {
        var html = await renderer.RenderComponent<SeasonNotification>(new Dictionary<string, object?>
        {
            { nameof(SeasonNotification.Notification), notification }
        });

        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            
            Arguments =
            [
                html
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