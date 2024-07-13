using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Web.BlazorComponents.SignalRContent;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public class OnOvasFeedNotification
{
    private readonly IStoreNotification _notificationStore;
    private readonly BlazorRenderer _renderer;
    private readonly ILogger<OnOvasFeedNotification> _logger;

    public OnOvasFeedNotification(
        IStoreNotification notificationStore,
        BlazorRenderer renderer,
        ILogger<OnOvasFeedNotification> logger)
    {
        _notificationStore = notificationStore;
        _renderer = renderer;
        _logger = logger;
    }

    [Function(nameof(OnOvasFeedNotification))]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(OvasFeedUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        OvasFeedUpdateNotification notification, CancellationToken token)
    {
        var result = await _notificationStore.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Ova,
            NotificationArea.Feed,
            notification, token);
        
        
        return await result.Match(
            _ =>
            {
                _logger.LogInformation("Ovas Feed Notification has been processed");
                return CreateMessage(notification);
            },
            e =>
            {
                e.LogError(_logger);
                return CreateMessage(notification);
            }
        );
    }
    
    
    private async Task<SignalRMessageAction> CreateMessage(OvasFeedUpdateNotification notification)
    {
        var html = await _renderer.RenderComponent<OvasFeedNotification>(new Dictionary<string, object?>
        {
            {nameof(OvasFeedNotification.Notification), notification}
        });

        return new SignalRMessageAction(ServerNotifications.FeedUpdates)
        {
            Arguments =
            [
                html
            ]
        };
    }
}