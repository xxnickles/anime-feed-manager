using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Notifications.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Series;

public class OnMoviesFeedNotification
{
    private readonly IStoreNotification _notificationStore;
    private readonly ILogger<OnMoviesFeedNotification> _logger;

    public OnMoviesFeedNotification(
        IStoreNotification notificationStore,
        ILogger<OnMoviesFeedNotification> logger)
    {
        _notificationStore = notificationStore;
        _logger = logger;
    }

    [Function(nameof(OnMoviesFeedNotification))]
    public async Task Run(
        [QueueTrigger(MoviesFeedUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        MoviesFeedUpdateNotification notification, CancellationToken token)
    {
        var result = await _notificationStore.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Movie,
            NotificationArea.Feed,
            notification, token);

        result.Match(
            _ => _logger.LogInformation("Movies Feed Notification has been processed"),
            error => error.LogError(_logger));
    }
}