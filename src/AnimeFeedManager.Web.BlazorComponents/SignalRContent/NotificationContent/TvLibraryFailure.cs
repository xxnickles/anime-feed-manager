using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

public static class TvLibraryFailure
{
    internal static RenderFragment ForTvLibraryScrapping(ScrapTvLibraryFailedResult notification) => builder =>
    {
        // Header
        builder.OpenElement(1, "h3");
        builder.AddAttribute(2, "class", "font-bold");
        builder.AddContent(3,
            $"TV library for {notification.Season} has been failed");
        builder.CloseElement(); // h3

        // Body
        builder.OpenElement(4, "p");
        builder.AddAttribute(5, "class", "text-sm whitespace-normal");
        builder.AddContent(6, "Scraping process has been failed.");
        builder.CloseElement(); // Close p
        
    };

}