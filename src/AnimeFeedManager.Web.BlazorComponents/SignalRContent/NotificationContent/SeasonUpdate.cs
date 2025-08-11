using AnimeFeedManager.Features.Seasons.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class SeasonUpdate
{
    internal static RenderFragment ForSeasonUpdate(SeasonUpdateResult notification) => builder =>
    {
        // Header
        builder.OpenElement(1, "h3");
        builder.AddAttribute(2, "class", "font-bold");
        
        builder.AddContent(3, GetNotificationTitle(notification));
        builder.CloseElement(); // h3

        // Body
        builder.OpenElement(4, "p");
        builder.AddAttribute(5, "class", "text-sm whitespace-normal");
        // First strong element
        builder.OpenElement(6, "strong");
        builder.AddContent(7, $"{notification.Season.Year}-{notification.Season.Season}");
        builder.CloseElement(); // Close strong
        // Text between strong elements
        builder.AddContent(8, GetNotificationBody(notification.SeasonUpdateStatus));
      
        builder.CloseElement(); // Close p
    };

    private static string GetNotificationTitle(SeasonUpdateResult notification) =>
        notification.SeasonUpdateStatus switch
        {
            SeasonUpdateStatus.New =>
                $"Season {notification.Season.Year}-{notification.Season.Season} has been created successfully",
            SeasonUpdateStatus.Updated =>
                $"Season {notification.Season.Year}-{notification.Season.Season} has been updated successfully",
            SeasonUpdateStatus.NoChanges =>
                $"Season {notification.Season.Year}-{notification.Season.Season} has been processed successfully",
            SeasonUpdateStatus.Error =>
                $"Season {notification.Season.Year}-{notification.Season.Season} process has failed",
            _ => throw new ArgumentOutOfRangeException(nameof(SeasonUpdateResult.SeasonUpdateStatus),
                notification.SeasonUpdateStatus, $"Unknown '{notification.SeasonUpdateStatus}' status")
        };
    
    private static string GetNotificationBody(SeasonUpdateStatus status) =>
        status switch
        {
            SeasonUpdateStatus.New => " has beed added to the system",
            SeasonUpdateStatus.Updated => " was already in the system and has been updated ",
            SeasonUpdateStatus.NoChanges => " was already in the system; no changes were made",
            SeasonUpdateStatus.Error => " update has failed",
            _ => throw new ArgumentOutOfRangeException(nameof(SeasonUpdateStatus),
                status, $"Unknown '{status}' status")
        };

}