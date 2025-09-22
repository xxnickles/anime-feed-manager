using AnimeFeedManager.Web.BlazorComponents.Toast;
using AnimeFeedManager.Web.Common;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

public readonly record struct Notification(
    string Title,
    RenderFragment Message,
    ToastType Type,
    TimeSpan? CloseTime = null);

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

    internal static Task<RazorComponentResult> ToComponentNotification<T, TViewModel, TComponent>(
        this Task<Result<T>> result,
        TViewModel viewModel)
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