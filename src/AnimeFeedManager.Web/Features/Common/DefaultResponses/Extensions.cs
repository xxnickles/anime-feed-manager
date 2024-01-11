﻿namespace AnimeFeedManager.Web.Features.Common.DefaultResponses;

internal static class Extensions
{
    internal static async Task<IResult> ToComponentResult(
        this Task<Either<DomainError, RenderedComponent>> result,
        BlazorRenderer renderer,
        ILogger logger,
        string okMessage
    )
    {
        var r = await result;
        
        var response = await r.Match(
            component => RenderOk(renderer,okMessage, component),
            error => RenderError(renderer, logger, error)
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
            _ => RenderOk(renderer,okMessage),
            error => RenderError(renderer, logger, error)
        );
        return Results.Content(response.Html, "text/html");
    }
    
    
    private static async Task<RenderedComponent> RenderOk(BlazorRenderer renderer, string message, params RenderedComponent[] additionalComponents)
    {
        var messageComponent = await RenderOk(renderer, message);
        return messageComponent.Combine(additionalComponents);
    }
  
    private static Task<RenderedComponent> RenderOk(BlazorRenderer renderer, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(OkResult.OkMessage), message}
        };

        return renderer.RenderComponent<OkResult>(parameters);
    }

    private static Task<RenderedComponent> RenderError(BlazorRenderer renderer, ILogger logger, DomainError error)
    {
        error.LogError(logger);
        
        var parameters = new Dictionary<string, object?>
        {
            {nameof(ErrorResult.Error), error}
        };

        return renderer.RenderComponent<ErrorResult>(parameters);
    }
}