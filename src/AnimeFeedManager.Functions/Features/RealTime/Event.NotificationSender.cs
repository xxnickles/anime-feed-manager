using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.RealTime;

public class NotificationSender
{

    private readonly ILogger<NotificationSender> _logger;
    public NotificationSender( ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<NotificationSender>();
    }
    
    [Function("NotificationSender")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public SignalRMessageAction Run([QueueTrigger(QueueNames.SeasonProcessNotifications, Connection = "AzureWebJobsStorage")] SeasonProcessNotification notification,
        FunctionContext context)
    {
        _logger.LogInformation("Notification ready to process {Notification}", notification);

        return new SignalRMessageAction(ServerNotifications.SeasonProcess)
        {
            GroupName = notification.TargetAudience == TargetAudience.Admins ? HubGroups.AdminGroup : null, 
            Arguments = new[] {notification}
        };
    }
}