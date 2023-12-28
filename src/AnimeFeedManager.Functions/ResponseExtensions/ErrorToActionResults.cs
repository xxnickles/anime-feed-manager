using AnimeFeedManager.Common.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.ResponseExtensions;

public static class ErrorToActionResults
{
    public static Task<HttpResponseData> ToResponse(this DomainError error, HttpRequestData request, ILogger log)
    {
        error.LogError(log);
        return error switch
        {
            ExceptionError eError => eError.ToResponse(request),
            ValidationErrors vError => vError.ToResponse(request),
            NotFoundError nError => nError.ToResponse(request),
            NoContentError cError => cError.ToResponse(request),
            UnauthorizedError uError => uError.ToResponse(request),
            ForbiddenError fError => fError.ToResponse(request),
            BasicError bError => bError.ToResponse(request),
            _ => request.InternalServerError("An unhandled error has occurred")
        };
    }

    private static Task<HttpResponseData> ToResponse(this ValidationErrors error, HttpRequestData request)
    {
        return request.UnprocessableEntity(error.Errors);
    }

    private static Task<HttpResponseData> ToResponse(this ExceptionError _, HttpRequestData request)
    {
        return request.InternalServerError("An internal error occurred");
    }

    private static Task<HttpResponseData> ToResponse(this NotFoundError error, HttpRequestData request)
    {
        return request.NotFound(error.Message);
    }

    private static Task<HttpResponseData> ToResponse(this NoContentError _, HttpRequestData request)
    {
        return request.NoContent();
    }

    private static Task<HttpResponseData> ToResponse(this UnauthorizedError _, HttpRequestData request)
    {
        return request.Unauthorized();
    }

    private static Task<HttpResponseData> ToResponse(this ForbiddenError _, HttpRequestData request)
    {
        return request.Forbidden();
    }

    private static Task<HttpResponseData> ToResponse(this BasicError error, HttpRequestData request)
    {
        return request.InternalServerError(error.ToString());
    }
}