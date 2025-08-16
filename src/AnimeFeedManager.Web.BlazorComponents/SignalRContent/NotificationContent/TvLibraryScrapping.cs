using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class TvLibraryScrapping
{
    internal static (string, RenderFragment) ForTvLibraryScrapping(ScrapTvLibraryResult notification) => (
        $"TV library for {notification.Season.Year}-{notification.Season.Season} has been scrapped successfully",
        builder =>
        {
            // First strong element
            builder.OpenElement(1, "strong");
            builder.AddContent(2, notification.NewSeries);
            builder.CloseElement(); // Close strong
            // Text between strong elements
            builder.AddContent(3, " new series has been added. ");
            // Second strong element
            builder.OpenElement(4, "strong");
            builder.AddContent(5, notification.UpdatedSeries);
            builder.CloseElement(); // Close strong
            // Final text
            builder.AddContent(6, " has been updated.");
        });
}