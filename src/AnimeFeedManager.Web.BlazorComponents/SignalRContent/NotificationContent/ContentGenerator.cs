using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Shared.Results.Errors;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class ContentGenerator
{
    internal static Result<(string Title, RenderFragment Message)> GetContent(object notification) => notification switch
    {
        ScrapTvLibraryResult scrappingResult => TvLibraryScrapping.ForTvLibraryScrapping(scrappingResult),
        ScrapTvLibraryFailedResult scrappingFailedResult => TvLibraryFailure.ForTvLibraryScrapping(
            scrappingFailedResult),
        SeasonUpdateResult seasonUpdateResult => SeasonUpdate.ForSeasonUpdate(seasonUpdateResult),
        _ => new OperationError(nameof(GetContent), "Unknown notification type")
    };
}