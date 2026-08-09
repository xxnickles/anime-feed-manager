using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Static;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ComponentResults
{
    internal static RazorComponentResult ToComponentResult<T>(this Result<T> result,
        Func<T, RenderFragment[]> onSuccess,
        Func<DomainError, RenderFragment[]> onError) =>
        result.MatchToValue<T, RazorComponentResult>(
            ok => onSuccess(ok).AggregateComponents(),
            error => onError(error).AggregateComponents());

    extension<T>(Task<Result<T>> result)
    {
        internal async Task<RazorComponentResult> ToComponentResult(
            Func<T, RenderFragment[]> onSuccess, Func<DomainError, RenderFragment[]> onError) =>
            (await result).ToComponentResult(onSuccess, onError);
    }
}
