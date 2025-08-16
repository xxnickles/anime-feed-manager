using AnimeFeedManager.Features.Tv.Library.Events;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

public static class TvLibraryFailure
{
    internal static (string, RenderFragment) ForTvLibraryScrapping(ScrapTvLibraryFailedResult notification) => (
        $"TV library for {notification.Season} has been failed",
        builder =>
        {
            // Body
            builder.AddContent(1, "Scraping process has been failed.");
        });
}