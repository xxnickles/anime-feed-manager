﻿using AnimeFeedManager.Common.Domain.Events;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Seasons;

public sealed class OnAddSeasonNotification
{
    private readonly AddSeasonNotificationHandler _addSeasonNotificationHandler;

    public OnAddSeasonNotification(AddSeasonNotificationHandler addSeasonNotificationHandler,
        ILoggerFactory loggerFactory)
    {
        _addSeasonNotificationHandler = addSeasonNotificationHandler;
        _logger = loggerFactory.CreateLogger<OnAddSeasonNotification>();
    }

    private readonly ILogger<OnAddSeasonNotification> _logger;

    [Function("OnAddSeasonNotification")]
    public async Task Run(
        [QueueTrigger(AddSeasonNotification.TargetQueue, Connection = "AzureWebJobsStorage")]
        AddSeasonNotification notification)
    {
        _logger.LogInformation("Updating Seasons with {Season}-{Year}", notification.Season, notification.Year.ToString());
        await _addSeasonNotificationHandler.Handle(notification, default);
    }
}