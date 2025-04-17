using AnimeFeedManager.Old.Common.Domain.Errors;

namespace AnimeFeedManager.Web.Features.Common.ApiResponses;

public static class ResultToApiResult
{
    public static Task<IResult> ToResponse<TResult>(this Task<Either<DomainError, TResult>> either, ILogger log)
    {
        return either.Map(x => x.ToResponse(log));
    }

    public static IResult ToResponse<TResult>(this Either<DomainError, TResult> either, ILogger log)
    {
        return either.Match(
            Left: error => error.ToResponse(log),
            Right: r => r is Unit ? Results.Accepted(): Results.Ok(r));
    }
}