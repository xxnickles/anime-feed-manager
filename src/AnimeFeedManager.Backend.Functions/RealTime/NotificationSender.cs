using AnimeFeedManager.Features.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.RealTime;

public class NotificationSender
{
    private readonly ILogger<NotificationSender> _logger;

    public NotificationSender(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<NotificationSender>();
    }

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
            Arguments = new object[] { notification }
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
            Arguments = new object[] { notification }
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
            Arguments = new object[] { notification }
        };
    }
}