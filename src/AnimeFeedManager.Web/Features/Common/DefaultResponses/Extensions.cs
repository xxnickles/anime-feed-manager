using AnimeFeedManager.Common.Domain.Errors;
using LanguageExt;

namespace AnimeFeedManager.Web.Features.Common.DefaultResponses;

internal static class Extensions
{
    internal static async Task<IResult> ToComponentResult(
        this Task<Either<DomainError, Unit>> result,
        BlazorRenderer renderer, 
        ILogger logger,
        string okMessage)
    {
        var r = await result;
        
        var response = await r.Match(
            _ => RenderOk(renderer,okMessage),
            error => RenderError(renderer, logger, error)
        );
        return Results.Content(response, "text/html");
    }
    
    private static Task<string> RenderOk(BlazorRenderer renderer, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(OkResult.OkMessage), message}
        };

        return renderer.RenderComponent<OkResult>(parameters);
    }

    private static Task<string> RenderError(BlazorRenderer renderer, ILogger logger, DomainError error)
    {
        error.LogDomainError(logger);
        
        var parameters = new Dictionary<string, object?>
        {
            {nameof(ErrorResult.Error), error}
        };

        return renderer.RenderComponent<ErrorResult>(parameters);
    }
}