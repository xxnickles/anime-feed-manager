using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

public static class TvLibraryFailure
{
    internal static RenderFragment ForTvLibraryScrapping(ScrapTvLibraryFailedResult notification) => builder =>
    {
        byte seq = 0;

        // Header
        builder.OpenElement(seq++, "h3");
        builder.AddAttribute(seq++, "class", "font-bold");
        builder.AddContent(seq++,
            $"TV library for {notification.Season} has been failed");
        builder.CloseElement(); // h3

        // Body
        builder.OpenElement(seq++, "p");
        builder.AddAttribute(seq++, "class", "text-sm whitespace-normal");
        builder.AddContent(seq, "Scraping process has been failed.");
        builder.CloseElement(); // Close p
        
    };

}