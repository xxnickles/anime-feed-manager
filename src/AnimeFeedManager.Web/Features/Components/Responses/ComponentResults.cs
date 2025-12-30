using AnimeFeedManager.Web.BlazorComponents;
using AnimeFeedManager.Web.BlazorComponents.Toast;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

public readonly record struct Notification(
    string Title,
    RenderFragment Message,
    ToastType Type,
    TimeSpan? CloseTime = null);

internal static class ComponentResults
{
    private static RazorComponentResult ToComponentResult<T>(this Result<T> result,
        Func<T, RenderFragment[]> onSuccess,
        Func<DomainError, RenderFragment[]> onError)
    {
        return result.MatchToValue<T,RazorComponentResult>(
            ok => onSuccess(ok).AggregateComponents(),
            error => onError(error).AggregateComponents());
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
                error =>
                [
                    TComponent.AsRenderFragment(viewModel),
                    Notifications.CreateErrorToast(TComponent.ErrorNotificationTitle, error)
                ]
            );
        }
    }
}