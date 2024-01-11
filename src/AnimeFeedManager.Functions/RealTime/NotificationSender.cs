using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.RealTime;

public class NotificationSender(ILoggerFactory loggerFactory)
{
    private readonly ILogger<NotificationSender> _logger = loggerFactory.CreateLogger<NotificationSender>();

    [Function("LibraryNotificationSender")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRMessageAction SendLibraryNotifications(
        [QueueTrigger(Box.Available.SeasonProcessNotificationsBox, Connection = "AzureWebJobsStorage")]
        SeasonProcessNotification notification,
        FunctionContext context)
    {
        _logger.LogInformation("Library notification ready to process {@Notification}", notification);

        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = notification.TargetAudience == TargetAudience.Admins ? HubGroups.AdminGroup : null,
            Arguments = [notification]
        };
    }

    [Function("TitleNotificationSender")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRMessageAction SendTitleNotifications(
        [QueueTrigger(Box.Available.TitleUpdatesNotificationsBox, Connection = "AzureWebJobsStorage")]
        TitlesUpdateNotification notification,
        FunctionContext context)
    {
        _logger.LogInformation("Title notification ready to process {@Notification}", notification);

        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = notification.TargetAudience == TargetAudience.Admins ? HubGroups.AdminGroup : null,
            Arguments = [notification]
        };
    }


    [Function("ImageNotificationSender")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRMessageAction SendImageNotifications(
        [QueueTrigger(Box.Available.ImageUpdateNotificationsBox, Connection = "AzureWebJobsStorage")]
        ImageUpdateNotification notification,
        FunctionContext context)
    {
        _logger.LogInformation("Image notification ready to process {@Notification}", notification);

        return new SignalRMessageAction(ServerNotifications.ImageUpdate)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments = [notification]
        };
    }
}