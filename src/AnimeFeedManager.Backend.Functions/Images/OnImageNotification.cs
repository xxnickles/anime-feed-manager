using AnimeFeedManager.Features.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;
using HubGroups = AnimeFeedManager.Common.HubGroups;
using ImageUpdateNotification = AnimeFeedManager.Features.Domain.Notifications.ImageUpdateNotification;
using NotificationType = AnimeFeedManager.Features.Domain.Notifications.NotificationType;

namespace AnimeFeedManager.Backend.Functions.Images;

public class OnImageNotification
{
    private readonly IStoreNotification _storeNotification;
    private readonly ILogger<OnImageNotification> _logger;

    public OnImageNotification(
        IStoreNotification storeNotification,
        ILoggerFactory loggerFactory)
    {
        _storeNotification = storeNotification;
        _logger = loggerFactory.CreateLogger<OnImageNotification>();
    }

    [Function("OnImageNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(Boxes.ImageUpdateNotifications, Connection = "AzureWebJobsStorage")] string myQueueItem,
        ImageUpdateNotification notification)
    {
        
        // Stores notification
        var result = await _storeNotification.Add(
            IdHelpers.GetUniqueId(),
            UserRoles.Admin,
            NotificationTarget.Admin,
            NotificationArea.Update,
            notification,
            default);


        return result.Match(
            _ =>
            {
                return new SignalRMessageAction(ServerNotifications.ImageUpdate)
                {
                    GroupName = HubGroups.AdminGroup,
                    Arguments = new object[]
                    {
                        new ImageUpdateNotification(
                            notification.Id,
                            NotificationType.Update,
                            notification.SeriesType,
                            notification.Message)
                    }
                };
            },
            e =>
            {
                e.LogDomainError(_logger);
                return new SignalRMessageAction(ServerNotifications.ImageUpdate)
                {
                    GroupName = HubGroups.AdminGroup,
                    Arguments = new object[]
                    {
                        new ImageUpdateNotification(
                            notification.Id,
                            NotificationType.Error,
                            notification.SeriesType,
                            "An Error occurred while updating images")
                    }
                };
            }
        );
    }
}