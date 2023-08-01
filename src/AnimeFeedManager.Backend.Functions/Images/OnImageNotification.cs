﻿using AnimeFeedManager.Features.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;
using ImageUpdateNotification = AnimeFeedManager.Features.Domain.Notifications.ImageUpdateNotification;

namespace AnimeFeedManager.Backend.Functions.Images;

public class OnImageNotification
{
    private readonly IStoreNotification _storeNotification;
    private readonly ILogger<OnImageNotification> _logger;

    public OnImageNotification(
        IStoreNotification storeNotification,
        ILoggerFactory loggerFactory)
    {
        _storeNotification = storeNotification;
        _logger = loggerFactory.CreateLogger<OnImageNotification>();
    }

    [Function("OnImageNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(Boxes.ImageUpdateNotifications, Connection = "AzureWebJobsStorage")] ImageUpdateNotification notification)
    {
        
        // Stores notification
        var result = await _storeNotification.Add(
            IdHelpers.GetUniqueId(),
            UserRoles.Admin,
            NotificationTarget.Images,
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
    
    private static SignalRMessageAction CreateMessage(ImageUpdateNotification notification)
    {
        return new SignalRMessageAction(ServerNotifications.ImageUpdate)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments = new object[]
            {
                notification
            }
        };
    }
}