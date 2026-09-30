using AnimeFeedManager.Web.BlazorComponents;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ComponentResults
{
    private static RazorComponentResult ToComponentResult<T>(this Result<T> result,
        Func<T, RenderFragment[]> onSuccess,
        Func<DomainError, RenderFragment[]> onError)
    {
        return result.MatchToValue<T,RazorComponentResult>(
            ok => onSuccess(ok).AggregateComponents(StatusCodes.Status200OK),
            error => onError(error).AggregateComponents(error.ToStatusCode()));
    }

    extension<T>(Task<Result<T>> result)
    {
        internal async Task<RazorComponentResult> ToComponentResult(Func<T, RenderFragment[]> onSuccess,
            Func<DomainError, RenderFragment[]> onError)
        {
            return (await result).ToComponentResult(onSuccess, onError);
        }

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
                error => [Notifications.CreateErrorToast(TComponent.ErrorNotificationTitle, error)]);
        }
    }
}