using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Extensions;

public static class ErrorToActionResults
{
    public static Task<HttpResponseData> ToResponse(this DomainError error, HttpRequestData request, ILogger log)
    {
        return error switch
        {
            ExceptionError eError => eError.ToResponse(request,log),
            ValidationErrors vError => vError.ToResponse(request,log),
            NotFoundError nError => nError.ToResponse(request,log),
            BasicError bError => bError.ToResponse(request,log),
            _ => request.InternalServerError("An unhandled error has ocurred")
        };
    }

    public static Task<HttpResponseData> ToResponse(this ValidationErrors error, HttpRequestData request, ILogger log)
    {
        if (error == null) return request.UnprocessableEntity();
        log.LogError("[{CorrelationId}] {Error}",error.CorrelationId, error.Message);
        foreach (var validationError in error.Errors)
            log.LogError("Field: {Field} Messages: {Messages}", validationError.Key, string.Join(". ", validationError.Value));

        return request.UnprocessableEntity(error.Errors);
    }

    public static Task<HttpResponseData> ToResponse(this ExceptionError error, HttpRequestData request, ILogger log)
    {
        log.LogError(error.Exception,"{Error}",error.ToString());
        return request.InternalServerError("An internal error occurred");
    }

    public static  Task<HttpResponseData> ToResponse(this NotFoundError error, HttpRequestData request, ILogger log)
    {
        log.LogError("{Error}", error.ToString());
        return request.NotFound(error.Message);
    }

    public static  Task<HttpResponseData> ToResponse(this BasicError error, HttpRequestData request, ILogger log)
    {
        log.LogError("{Error}", error.ToString());
        return request.InternalServerError(error.ToString());
    }
}