using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Web.Common;

internal static class ResponseExtensions
{
    internal static IResult ToJsonResponse<T>(this Result<T> result, ILogger logger)
    {
        return result.MatchToValue(
            success => success.ToSuccessResponse(),
            error => error.ToErrorResponse(logger)
        );
    }

    internal static async Task<IResult> ToJsonResponse<T>(this Task<Result<T>> result, ILogger logger)
    {
        return (await result).ToJsonResponse(logger);
    }


    internal static IResult ToJsonResponse(this Result<Unit> result, ILogger logger)
    {
        return result.MatchToValue(
            _ => TypedResults.Accepted(string.Empty),
            error => error.ToErrorResponse(logger)
        );
    }
    
    internal static async Task<IResult> ToJsonResponse(this Task<Result<Unit>> result, ILogger logger)
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
        error.LogError(logger);
        return error switch
        {
            NotFoundError => TypedResults.NotFound(),
            DomainValidationErrors validationErrors => TypedResults.ValidationProblem(
                validationErrors.Errors.ToDictionary(e => e.Field, e => e.Errors),
                detail: "One or more validation errors occurred",
                title: "Validation Error"),
            ExceptionError => TypedResults.Problem(
                detail: "An internal server error occurred",
                title: "Internal Server Error",
                statusCode: 500,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"),
            HandledError => TypedResults.NoContent(),
            OperationError operationError => TypedResults.Problem(
                detail: operationError.Message,
                title: $"Operation '{operationError.Operation}' failed",
                statusCode: 400,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            TypeMismatch typeMismatch => TypedResults.Problem(
                detail:
                $"Expected type '{typeMismatch.TargetType.Name}' but received '{typeMismatch.Received.GetType().Name}'",
                title: "Type Mismatch Error",
                statusCode: 400,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1"),
            AggregatedError aggregatedError => TypedResults.Problem(
                detail: string.Join("; ", aggregatedError.Errors.Select(e => e.Message)),
                title: aggregatedError.FailureType == FailureType.Partial
                    ? "Some operations failed"
                    : "All operations failed",
                statusCode: aggregatedError.FailureType == FailureType.Partial ? 207 : 500,
                type: "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1"),
            Error basicError => TypedResults.Problem(
                detail: basicError.Message,
                title: "An error occurred",
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