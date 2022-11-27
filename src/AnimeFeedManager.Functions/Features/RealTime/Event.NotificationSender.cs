using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.RealTime;

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
        [QueueTrigger(Boxes.SeasonProcessNotifications, Connection = "AzureWebJobsStorage")]
        SeasonProcessNotification notification,
        FunctionContext context)
    {
        _logger.LogInformation("Library notification ready to process {Notification}", notification);

        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = notification.TargetAudience == TargetAudience.Admins ? HubGroups.AdminGroup : null,
            Arguments = new object[] {notification}
        };
    }

    [Function("TitleNotificationSender")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRMessageAction SendTitleNotifications(
        [QueueTrigger(Boxes.TitleUpdatesNotifications, Connection = "AzureWebJobsStorage")]
        SeasonProcessNotification notification,
        FunctionContext context)
    {
        _logger.LogInformation("Title notification ready to process {Notification}", notification);

        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = notification.TargetAudience == TargetAudience.Admins ? HubGroups.AdminGroup : null,
            Arguments = new object[] {notification}
        };
    }
}