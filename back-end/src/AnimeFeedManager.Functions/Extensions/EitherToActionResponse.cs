using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Extensions;

public static class EitherToActionResponse
{
    public static Task<HttpResponseData> ToResponse<TResult>(this Task<Either<DomainError, TResult>> either,
        HttpRequestData request, ILogger log)
    {
       return either.MapAsync( x => ToResponse(x, request, log));
    }
       

    public static Task<HttpResponseData> ToResponse<TResult>(this Either<DomainError, TResult> either, HttpRequestData request,
        ILogger log)
    {
        return either.Match(
            Left: error => error.ToResponse(request, log),
            Right: r => r is Unit ? request.Ok(): request.Ok(r));
    }
        


}