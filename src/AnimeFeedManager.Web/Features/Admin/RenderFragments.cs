using AnimeFeedManager.Features.Users.Types;
using AnimeFeedManager.Web.Features.Common;

namespace AnimeFeedManager.Web.Features.Admin;

internal static class RenderFragments
{
    internal static async Task<IResult> ToComponentResult(
        this Task<Either<DomainError, UsersCheck>> result,
        BlazorRenderer renderer,
        ILogger logger,
        string okMessage)
    {
        var r = await result;

        var response = await r.Match(
            check => RenderOk(renderer, check, okMessage, logger),
            error => Common.DefaultResponses.Extensions.RenderError(renderer, logger, error)
        );
        return Results.Content(response.Html, "text/html");
    }

    private static Task<RenderedComponent> RenderOk(BlazorRenderer renderer, UsersCheck result, string message,  ILogger logger)
    {
        return result switch
        {
            AllMatched => Common.DefaultResponses.Extensions.RenderOk(renderer, message),
            SomeNotFound s => RenderUserNotFound(renderer, s),
            _ => Common.DefaultResponses.Extensions.RenderError(renderer, logger,
                BasicError.Create($"{nameof(UsersCheck)} is out of range"))
        };
    }


    private static Task<RenderedComponent> RenderUserNotFound(BlazorRenderer renderer, SomeNotFound notFound)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UserNotFoundResult.Condition), notFound}
        };

        return renderer.RenderComponent<UserNotFoundResult>(parameters);
    }
}