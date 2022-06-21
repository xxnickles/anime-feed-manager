using AnimeFeedManager.Core.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Web.Http;

namespace AnimeFeedManager.Functions.Extensions;

public static class ErrorToActionResults
{
    public static IActionResult ToActionResult(this DomainError error, ILogger log)
    {
        return error switch
        {
            ExceptionError eError => eError.ToActionResult(log),
            ValidationErrors vError => vError.ToActionResult(log),
            NotFoundError nError => nError.ToActionResult(log),
            BasicError bError => bError.ToActionResult(log),
            _ => new InternalServerErrorResult()
        };
    }

    public static IActionResult ToActionResult(this ValidationErrors error, ILogger log)
    {
        if (error == null) return new UnprocessableEntityResult();
        log.LogError($"[{error.CorrelationId}] {error.Message}");
        foreach (var validationError in error.Errors)
            log.LogError($"Field: {validationError.Key}, Messages: {string.Join(". ", validationError.Value)}");

        // var problemDetails = new ProblemDetails(error.Errors); TODO: Works on MVC Core 3.0

        return new UnprocessableEntityObjectResult(new
        {
            Title = error.Message,
            error.Errors
        });

    }

    public static IActionResult ToActionResult(this ExceptionError error, ILogger log)
    {
        log.LogError(error.Exception,error.ToString());
        return new InternalServerErrorResult();
    }

    public static IActionResult ToActionResult(this NotFoundError error, ILogger log)
    {
        log.LogError(error.ToString());
        return new NotFoundObjectResult(new { Message = error.Message });
    }

    public static IActionResult ToActionResult(this BasicError error, ILogger log)
    {
        log.LogError(error.ToString());
        return new InternalServerErrorResult();
    }
}