using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.ResponseExtensions;

public static class EitherToActionResponse
{
    public static Task<HttpResponseData> ToResponse<TResult>(this Task<Either<DomainError, TResult>> either,
        HttpRequestData request, ILogger log)
    {
       return either.MapAsync( x => ToResponse(x, request, log));
    }
       

    public static Task<HttpResponseData> ToResponse<TResult>(this Either<DomainError, TResult> either, HttpRequestData request, ILogger log)
    {
        return either.Match(
            Left: error => error.ToResponse(request, log),
            Right: r => r is Unit ? request.Accepted(): request.Ok(r));
    }
}