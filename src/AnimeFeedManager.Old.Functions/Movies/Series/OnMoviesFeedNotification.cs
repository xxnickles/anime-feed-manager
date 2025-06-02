using AnimeFeedManager.Old.Common.Domain.Notifications.Base;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.RealTimeNotifications;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Notifications.IO;
using AnimeFeedManager.Web.BlazorComponents.SignalRContent;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Movies.Series;

public class OnMoviesFeedNotification
{
    private readonly IStoreNotification _notificationStore;
    private readonly BlazorRenderer _renderer;
    private readonly ILogger<OnMoviesFeedNotification> _logger;

    public OnMoviesFeedNotification(
        IStoreNotification notificationStore,
        BlazorRenderer renderer,
        ILogger<OnMoviesFeedNotification> logger)
    {
        _notificationStore = notificationStore;
        _renderer = renderer;
        _logger = logger;
    }

    [Function(nameof(OnMoviesFeedNotification))]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(MoviesFeedUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        MoviesFeedUpdateNotification notification, CancellationToken token)
    {
        var result = await _notificationStore.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Movie,
            NotificationArea.Feed,
            notification, token);

        return await result.Match(
            _ =>
            {
                _logger.LogInformation("Movies Feed Notification has been processed");
                return CreateMessage(notification);
            },
            e =>
            {
                e.LogError(_logger);
                return CreateMessage(notification);
            }
        );
    }

    private async Task<SignalRMessageAction> CreateMessage(MoviesFeedUpdateNotification notification)
    {
        var html = await _renderer.RenderComponent<MoviesFeedNotification>(new Dictionary<string, object?>
        {
            {nameof(MoviesFeedNotification.Notification), notification}
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