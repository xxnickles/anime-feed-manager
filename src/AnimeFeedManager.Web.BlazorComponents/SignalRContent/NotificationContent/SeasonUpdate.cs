using AnimeFeedManager.Features.Seasons.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class SeasonUpdate
{
    internal static RenderFragment ForSeasonUpdate(SeasonUpdateResult notification) => builder =>
    {
        byte seq = 0;
        
        // Header
        builder.OpenElement(seq++, "h3");
        builder.AddAttribute(seq++, "class", "font-bold");
        
        builder.AddContent(seq++, GetNotificationTitle(notification));
        builder.CloseElement(); // h3

        // Body
        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "class", "text-sm whitespace-normal");
        // First strong element
        builder.OpenElement(seq++, "strong");
        builder.AddContent(seq++, $"{notification.Season.Year}-{notification.Season.Season}");
        builder.CloseElement(); // Close strong
        // Text between strong elements
        builder.AddContent(seq++, GetNotificationBody(notification.SeasonUpdateStatus));
      
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