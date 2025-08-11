using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class TvLibraryScrapping
{
    internal static RenderFragment ForTvLibraryScrapping(ScrapTvLibraryResult notification) => builder =>
    {
        // Header
        builder.OpenElement(1, "h3");
        builder.AddAttribute(2, "class", "font-bold");
        builder.AddContent(3,
            $"TV library for {notification.Season.Year}-{notification.Season.Season} has been scrapped successfully");
        builder.CloseElement(); // h3

        // Body
        builder.OpenElement(4, "p");
        builder.AddAttribute(5, "class", "text-sm whitespace-normal");
        // First strong element
        builder.OpenElement(6, "strong");
        builder.AddContent(7, notification.NewSeries);
        builder.CloseElement(); // Close strong
        // Text between strong elements
        builder.AddContent(8, " new series has been added. ");
        // Second strong element
        builder.OpenElement(9, "strong");
        builder.AddContent(10, notification.UpdatedSeries);
        builder.CloseElement(); // Close strong
        // Final text
        builder.AddContent(11, " has been updated.");
        builder.CloseElement(); // Close p
    };
}