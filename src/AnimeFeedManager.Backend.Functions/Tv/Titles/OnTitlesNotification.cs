using AnimeFeedManager.Backend.Functions.Images;
using AnimeFeedManager.Features.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;
using ImageUpdateNotification = AnimeFeedManager.Features.Domain.Notifications.ImageUpdateNotification;

namespace AnimeFeedManager.Backend.Functions.Tv.Titles;

public class OnTitlesNotification
{
    private readonly IStoreNotification _storeNotification;
    private readonly ILogger<OnTitlesNotification> _logger;

    public OnTitlesNotification(
        IStoreNotification storeNotification,
        ILoggerFactory loggerFactory)
    {
        _storeNotification = storeNotification;
        _logger = loggerFactory.CreateLogger<OnTitlesNotification>();
    }

    [Function("OnTitlesNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(Boxes.TitleUpdatesNotifications, Connection = "AzureWebJobsStorage")] TitlesUpdateNotification notification)
    {
        
        // Stores notification
        var result = await _storeNotification.Add(
            IdHelpers.GetUniqueId(),
            UserRoles.Admin,
            NotificationTarget.Tv,
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
    
    private static SignalRMessageAction CreateMessage(TitlesUpdateNotification notification)
    {
        return new SignalRMessageAction(ServerNotifications.TitleUpdate)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments = new object[]
            {
                notification
            }
        };
    }
}