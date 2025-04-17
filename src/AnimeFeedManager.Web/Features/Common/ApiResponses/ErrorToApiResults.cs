using System.Net;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Users.Types;

namespace AnimeFeedManager.Web.Features.Common.ApiResponses;

public static class ErrorToApiResults
{
    public static IResult ToResponse(this DomainError error, ILogger log)
    {
        error.LogError(log);
        return error switch
        {
            ExceptionError eError => eError.ToResponse(),
            ValidationErrors vError => vError.ToResponse(),
            NotFoundError nError => nError.ToResponse(),
            NoContentError cError => cError.ToResponse(),
            BasicError bError => bError.ToResponse(),
            PasswordlessError pError => pError.ToResponse(),
            _ => DefaultError()
        };
    }


    private static IResult ToResponse(this ValidationErrors error)
    {
        return Results.ValidationProblem(
            error.Errors,
            string.Empty,
            string.Empty,
            (int)HttpStatusCode.UnprocessableEntity,
            "An error occurred when processing your request.",
            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422");
    }

    private static IResult ToResponse(this ExceptionError _)
    {
        return Results.Problem(
            null,
            null,
            (int)HttpStatusCode.InternalServerError,
            "A system error has occurred",
            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500");
    }

    private static IResult ToResponse(this NotFoundError _)
    {
        return Results.NotFound();
    }

    private static IResult ToResponse(this NoContentError _)
    {
        return Results.NoContent();
    }


    private static IResult ToResponse(this BasicError error)
    {
        return Results.Problem(
            error.ToString(),
            null,
            (int)HttpStatusCode.InternalServerError,
            "A error has ocurred",
            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500");
    }

    private static IResult ToResponse(this PasswordlessError error)
    {
        return Results.Problem(
            detail: error.Exception.Details.Detail,
            title: error.Exception.Details.Title,
            type: error.Exception.Details.Type,
            statusCode: error.Exception.Details.Status,
            instance: error.Exception.Details.Instance,
            extensions: error.Exception.Details.Extensions.ToDictionary(
                pair => pair.Key,
                pair => (object?)pair.Value
            ));
    }


    private static IResult DefaultError() => Results.Problem(
        null,
        null,
        (int)HttpStatusCode.InternalServerError,
        "A error has ocurred",
        "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500");
}