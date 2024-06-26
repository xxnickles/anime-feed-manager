﻿using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.RealTimeNotifications;
using AnimeFeedManager.Features.Notifications.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Titles;

public sealed class OnTitlesNotification(
    IStoreNotification storeNotification,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnTitlesNotification> _logger = loggerFactory.CreateLogger<OnTitlesNotification>();

    [Function("OnTitlesNotification")]
    [SignalROutput(HubName = HubNames.Notifications, ConnectionStringSetting = "SignalRConnectionString")]
    public async Task<SignalRMessageAction> Run(
        [QueueTrigger(TitlesUpdateNotification.TargetQueue, Connection = Constants.AzureConnectionName)] 
        TitlesUpdateNotification notification,
        CancellationToken token)
    {
        
        // Stores notification
        var result = await storeNotification.Add(
            IdHelpers.GetUniqueId(),
            RoleNames.Admin,
            NotificationTarget.Tv,
            NotificationArea.Update,
            notification,
            token);


        return result.Match(
            _ => CreateMessage(notification),
            e =>
            {
                e.LogError(_logger);
                return CreateMessage(notification);
            }
        );
    }
    
    private static SignalRMessageAction CreateMessage(TitlesUpdateNotification notification)
    {
        return new SignalRMessageAction(ServerNotifications.TitleUpdate)
        {
            GroupName = HubGroups.AdminGroup,
            Arguments =
            [
                notification
            ]
        };
    }
}