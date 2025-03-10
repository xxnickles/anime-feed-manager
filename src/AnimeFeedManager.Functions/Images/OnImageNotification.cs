﻿using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;
using ImageUpdateNotification = AnimeFeedManager.Common.Domain.Notifications.ImageUpdateNotification;

namespace AnimeFeedManager.Functions.Images;

public sealed class OnImageNotification(
    IStoreNotification storeNotification,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnImageNotification> _logger = loggerFactory.CreateLogger<OnImageNotification>();

    [Function(nameof(OnImageNotification))]
    public async Task Run(
        [QueueTrigger(ImageUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)]
        ImageUpdateNotification notification,
        CancellationToken token)
    {
        // Stores notification
        var result = await storeNotification.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Images,
            NotificationArea.Update,
            notification,
            token);


        result.Match(
            _ => _logger.LogInformation(
                "Image notification for {For} has been stored. Notification message is {Message}",
                notification.SeriesType, notification.Message),
            e => e.LogError(_logger)
        );
    }
}