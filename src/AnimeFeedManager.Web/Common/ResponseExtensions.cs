namespace AnimeFeedManager.Web.Common;

internal static class ResponseExtensions
{
    internal static IResult ToJsonResponse<T>(this Result<T, DomainError> result, ILogger logger)
    {
        return result.MatchToValue(
            success => success.ToSuccessResponse(),
            error => error.ToErrorResponse(logger)
        );
    }

    internal static async Task<IResult> ToJsonResponse<T>(this Task<Result<T, DomainError>> result, ILogger logger)
    {
        return (await result).ToJsonResponse(logger);
    }


    internal static IResult ToJsonResponse(this Result<Unit, DomainError> result, ILogger logger)
    {
        return result.MatchToValue(
            _ => TypedResults.Accepted(string.Empty),
            error => error.ToErrorResponse(logger)
        );
    }
    
    internal static async Task<IResult> ToJsonResponse(this Task<Result<Unit, DomainError>> result, ILogger logger)
    {
        return (await result).ToJsonResponse(logger);
    }

    private static IResult ToSuccessResponse<T>(this T value)
    {
        return value switch
        {
            Unit => TypedResults.Accepted(string.Empty),
            null => TypedResults.NoContent(),
            _ => TypedResults.Ok(value)
        };
    }

    private static IResult ToErrorResponse(this DomainError error, ILogger logger)
    {
        return error switch
        {
            StorageError.NotFound => TypedResults.NotFound(),
            ValidationError.Fields fields => TypedResults.ValidationProblem(
                fields.Errors.ToDictionary(e => e.Field, e => e.Errors.ToArray()),
                detail: "One or more validation errors occurred",
                title: "Validation Error"),
            StorageError.RequestFailed => TypedResults.Problem(
                detail: "An internal server error occurred",
                title: "Internal Server Error",
                statusCode: 500,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"),
            AggregatedError aggregatedError => TypedResults.Problem(
                detail: string.Join("; ", aggregatedError.Errors.Select(e => e.Message)),
                title: "All operations failed",
                statusCode: 500,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"),
            _ => TypedResults.Problem(
                detail: "An unexpected error occurred",
                title: "Unexpected Error",
                statusCode: 500,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1")
        };
    }
}