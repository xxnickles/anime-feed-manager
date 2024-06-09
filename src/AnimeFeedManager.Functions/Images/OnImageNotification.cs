using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;
using ImageUpdateNotification = AnimeFeedManager.Common.Domain.Notifications.ImageUpdateNotification;

namespace AnimeFeedManager.Functions.Images;

public sealed class OnImageNotification(
    IStoreNotification storeNotification,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnImageNotification> _logger = loggerFactory.CreateLogger<OnImageNotification>();

    [Function("OnImageNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(ImageUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)] ImageUpdateNotification notification,
        CancellationToken token)
    {
        
        // Stores notification
        var result = await storeNotification.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Images,
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
    
    private static SignalRMessageAction CreateMessage(ImageUpdateNotification notification)
    {
        return new SignalRMessageAction(ServerNotifications.ImageUpdate)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments =
            [
                notification
            ]
        };
    }
}