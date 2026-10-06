using AnimeFeedManager.Features.User.Authentication;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ErrorStatusCodes
{
    // Mapped as error types reach the client; anything unmapped is a 500.
    internal static int ToStatusCode(this DomainError error) => error switch
    {
        NotFoundError => StatusCodes.Status404NotFound,
        DomainValidationErrors or FormDataValidationError => StatusCodes.Status422UnprocessableEntity,
        // Upstream 4xx is the caller's problem (e.g. an invalid token); anything else is Passwordless failing.
        PasswordlessError { ProblemDetails.Status: >= 400 and < 500 } passwordless => passwordless.ProblemDetails.Status,
        PasswordlessError => StatusCodes.Status502BadGateway,
        _ => StatusCodes.Status500InternalServerError
    };
}
