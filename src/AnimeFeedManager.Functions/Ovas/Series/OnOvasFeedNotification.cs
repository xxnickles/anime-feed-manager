using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public class OnOvasFeedNotification
{
    private readonly IStoreNotification _notificationStore;
    private readonly ILogger<OnOvasFeedNotification> _logger;

    public OnOvasFeedNotification(
        IStoreNotification notificationStore,
        ILogger<OnOvasFeedNotification> logger)
    {
        _notificationStore = notificationStore;
        _logger = logger;
    }

    [Function(nameof(OnOvasFeedNotification))]
    public async Task Run(
        [QueueTrigger(OvasFeedUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        OvasFeedUpdateNotification notification, CancellationToken token)
    {
        var result = await _notificationStore.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Ova,
            NotificationArea.Feed,
            notification, token);

        result.Match(
            _ => _logger.LogInformation("Ovas Feed Notification has been processed"),
            error => error.LogError(_logger));
    }
}