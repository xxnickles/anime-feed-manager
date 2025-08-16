using AnimeFeedManager.Features.Seasons.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class SeasonUpdate
{
    internal static (string, RenderFragment) ForSeasonUpdate(SeasonUpdateResult notification) => (
        GetNotificationTitle(notification),
        builder =>
        {
            // First strong element
            builder.OpenElement(1, "strong");
            builder.AddContent(2, $"{notification.Season.Year}-{notification.Season.Season}");
            builder.CloseElement(); // Close strong
            // Text between strong elements
            builder.AddContent(3, GetNotificationBody(notification.SeasonUpdateStatus));
        });

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