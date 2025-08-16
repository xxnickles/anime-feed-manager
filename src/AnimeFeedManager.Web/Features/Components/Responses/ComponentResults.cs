using AnimeFeedManager.Web.BlazorComponents.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Components.Responses;

public readonly record struct Notification(string Title, RenderFragment Message, ToastType Type, TimeSpan? CloseTime = null);

internal static class ComponentResults
{
    internal static RazorComponentResult ToComponentResult<T>(this Result<T> result,
        Func<T, RenderFragment[]> onSuccess,
        Func<DomainError, RenderFragment[]> onError)
    {
        return result.MatchToValue<RazorComponentResult>(
            ok => onSuccess(ok).AggregateComponents(),
            error => onError(error).AggregateComponents());
    }

    internal static async Task<RazorComponentResult> ToComponentResult<T>(this Task<Result<T>> result,
        Func<T, RenderFragment[]> onSuccess,
        Func<DomainError, RenderFragment[]> onError)
    {
        return (await result).ToComponentResult(onSuccess, onError);
    }
  
}