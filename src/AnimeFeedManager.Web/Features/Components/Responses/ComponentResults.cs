using AnimeFeedManager.Web.BlazorComponents;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ComponentResults
{
    private static RazorComponentResult ToComponentResult<T, TError>(this Result<T, TError> result,
        Func<T, RenderFragment[]> onSuccess,
        Func<DomainError, RenderFragment[]> onError) where TError : DomainError
    {
        return result.MatchToValue(
            ok => onSuccess(ok).AggregateComponents(StatusCodes.Status200OK),
            error => onError(error).AggregateComponents(error.ToStatusCode()));
    }

    extension<T, TError>(Task<Result<T, TError>> result) where TError : DomainError
    {
        internal async Task<RazorComponentResult> ToComponentResult(Func<T, RenderFragment[]> onSuccess,
            Func<DomainError, RenderFragment[]> onError)
        {
            return (await result).ToComponentResult(onSuccess, onError);
        }
    }

    // Separate block: callers pass explicit type arguments, which span the block's type parameters.
    extension<T>(Task<Result<T, DomainError>> result)
    {
        internal Task<RazorComponentResult> ToComponentNotification<TViewModel, TComponent>(TViewModel viewModel)
            where TViewModel : class, new()
            where TComponent : INotifiableComponent<TViewModel>
        {
            return result.ToComponentResult(
                _ =>
                [
                    TComponent.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast(
                        TComponent.SuccessNotificationTitle,
                        TComponent.OkNotificationContent(viewModel))
                ],
                error => [Notifications.CreateErrorNotification(TComponent.ErrorNotificationTitle, error)]);
        }
    }
}