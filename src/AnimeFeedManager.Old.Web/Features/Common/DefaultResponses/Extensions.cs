using AnimeFeedManager.Old.Common.Domain.Errors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Old.Web.Features.Common.DefaultResponses;

internal static class Extensions
{
    internal static async Task<IResult> ToComponentResult<T>(
        this Task<Either<DomainError, T>> result, Func<T, RazorComponentResult> onOk,
        Func<DomainError, RazorComponentResult> onError)

    {
        var r = await result;
        return r.Match(
            onOk,
            onError
        );
    }

    internal static Task<IResult> ToComponentResult(
        this Task<Either<DomainError, Unit>> result,
        string okMessage,
        ILogger logger
    )
    {
        return result.ToComponentResult(
            _ => CommonComponentResponses.OkComponentResult(okMessage),
            error => CommonComponentResponses.ErrorComponentResult(error, logger));
    }

}