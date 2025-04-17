using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Types;
using AnimeFeedManager.Web.Features.Movies.Controls;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Movies;

public static class ComponentResponses
{
    internal static RazorComponentResult OkSubscribedResponse(MovieControlData data, string message)

    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(SubscribedMovieControls.ControlData), data},
            {nameof(SubscribedMovieControls.Message), message}
        };
        return new RazorComponentResult<SubscribedMovieControls>(parameters);
    }
    
    internal static RazorComponentResult OkUnsubscribedResponse(MovieControlData data, string message)

    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UnsubscribedMovieControls.ControlData), data},
            {nameof(UnsubscribedMovieControls.Message), message}
        };
        return new RazorComponentResult<UnsubscribedMovieControls>(parameters);
    }

    internal static RazorComponentResult ErrorResponse(MovieControlData data, DomainError domainError,
        ILogger logger)
    {
        domainError.LogError(logger);
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UnsubscribedMovieControls.ControlData), data},
            {nameof(UnsubscribedMovieControls.DomainError), domainError}
        };
        return new RazorComponentResult<UnsubscribedMovieControls>(parameters);
    }
    
    internal static RazorComponentResult OkResponse(SeasonInformation seasonInformation, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(MoviesGridComponent.SeasonInfo), seasonInformation},
            {nameof(MoviesGridComponent.Message), message}
        };
        return new RazorComponentResult<MoviesGridComponent>(parameters);
    }
}