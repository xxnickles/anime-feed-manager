using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class TvLibraryScrapping
{
    internal static RenderFragment ForTvLibraryScrapping(ScrapTvLibraryResult notification) => builder =>
    {
        byte seq = 0;

        // Header
        builder.OpenElement(seq++, "h3");
        builder.AddAttribute(seq++, "class", "font-bold");
        builder.AddContent(seq++,
            $"TV library for {notification.Season.Year}-{notification.Season.Season} has been scrapped successfully");
        builder.CloseElement(); // h3

        // Body
        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "class", "text-sm whitespace-normal");
        // First strong element
        builder.OpenElement(seq++, "strong");
        builder.AddContent(seq++, notification.NewSeries);
        builder.CloseElement(); // Close strong
        // Text between strong elements
        builder.AddContent(seq++, " new series has been added. ");
        // Second strong element
        builder.OpenElement(seq++, "strong");
        builder.AddContent(seq++, notification.UpdatedSeries);
        builder.CloseElement(); // Close strong
        // Final text
        builder.AddContent(seq, " has been updated.");
        builder.CloseElement(); // Close p
    };
}