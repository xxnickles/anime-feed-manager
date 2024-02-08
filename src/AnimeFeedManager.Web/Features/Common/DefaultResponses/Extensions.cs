using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Common.DefaultResponses;

internal static class Extensions
{
    internal static async Task<IResult> ToComponentResult<T>(
        this Task<Either<DomainError, T>> result, Func<T, RazorComponentResult> onOk,
        Func<DomainError, RazorComponentResult> onError)

    {
        var r = await result;
        return r.Match(
            onOk,
            onError
        );
    }

    internal static Task<IResult> ToComponentResult(
        this Task<Either<DomainError, Unit>> result,
        string okMessage,
        ILogger logger
    )
    {
        return result.ToComponentResult(
            _ => CommonComponentResponses.OkComponentResult(okMessage),
            error => CommonComponentResponses.ErrorComponentResult(error, logger));
    }


    internal static async Task<IResult> ToComponentResult(
        this Task<Either<DomainError, RenderedComponent>> result,
        BlazorRenderer renderer,
        ILogger logger,
        string okMessage,
        params Task<RenderedComponent>[] additionalErrorComponents)
    {
        var r = await result;

        var response = await r.Match(
            component => RenderOk(renderer, okMessage, component),
            error => RenderError(renderer, logger, error, additionalErrorComponents)
        );
        return Results.Content(response.Html, "text/html");
    }

    internal static async Task<IResult> ToComponentResult(
        this Task<Either<DomainError, Unit>> result,
        BlazorRenderer renderer,
        ILogger logger,
        string okMessage)
    {
        var r = await result;

        var response = await r.Match(
            _ => RenderOk(renderer, okMessage),
            error => RenderError(renderer, logger, error)
        );
        return Results.Content(response.Html, "text/html");
    }


    private static async Task<RenderedComponent> RenderOk(BlazorRenderer renderer, string message,
        params RenderedComponent[] additionalComponents)
    {
        var messageComponent = await RenderOk(renderer, message);
        return messageComponent.Combine(additionalComponents);
    }

    internal static Task<RenderedComponent> RenderOk(BlazorRenderer renderer, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(OkResult.OkMessage), message}
        };

        return renderer.RenderComponent<OkResult>(parameters);
    }

    internal static Task<RenderedComponent> RenderError(BlazorRenderer renderer, ILogger logger, DomainError error)
    {
        error.LogError(logger);

        var parameters = new Dictionary<string, object?>
        {
            {nameof(ErrorResult.Error), error}
        };

        return renderer.RenderComponent<ErrorResult>(parameters);
    }

    private static async Task<RenderedComponent> RenderError(BlazorRenderer renderer, ILogger logger, DomainError error,
        params Task<RenderedComponent>[] additionalErrorComponents)
    {
        var additionalComponents = await Task.WhenAll(additionalErrorComponents);
        var messageComponent = await RenderError(renderer, logger, error);
        return messageComponent.Combine(additionalComponents);
    }
}