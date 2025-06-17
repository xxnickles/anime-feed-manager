using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Shared.Results.Errors;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.BlazorComponents.SignalRContent.NotificationContent;

internal static class ContentGenerator
{
    internal static Result<RenderFragment> GetContent(object notification) => notification switch
    {
        ScrapTvLibraryResult scrappingResult => Result<RenderFragment>.Success(TvLibraryScrapping.ForTvLibraryScrapping(scrappingResult)),
        ScrapTvLibraryFailedResult scrappingFailedResult => Result<RenderFragment>.Success(TvLibraryFailure.ForTvLibraryScrapping(scrappingFailedResult)),
        SeasonUpdateResult seasonUpdateResult => Result<RenderFragment>.Success(SeasonUpdate.ForSeasonUpdate(seasonUpdateResult)), 
        _ => Result<RenderFragment>.Failure(new OperationError(nameof(GetContent), "Unknown notification type" ))
    };
}