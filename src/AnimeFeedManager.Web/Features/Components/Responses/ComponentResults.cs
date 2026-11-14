using System.Runtime.CompilerServices;
using AnimeFeedManager.Web.BlazorComponents;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ComponentResults
{
    extension<T, TError>(Task<Result<T, TError>> result) where TError : DomainError
    {
        internal Task<RazorComponentResult> ToComponentResult(Func<T, RenderFragment[]> onSuccess,
            Func<DomainError, RenderFragment[]> onError)
        {
            return result.MatchToValue(
                ok => onSuccess(ok).AggregateComponents(StatusCodes.Status200OK),
                error => onError(error).AggregateComponents(error.ToStatusCode()));
        }
    }

    extension<T, TError>(Task<Result<T, TError>> result)
    {
        internal PendingNotification<T, TError, TViewModel> ToComponentNotification<TViewModel>(TViewModel viewModel,
            Func<TError, ErrorContent> toContent)
            where TViewModel : class, new() => new(result, viewModel, toContent);
    }
}

/// <summary>A notification awaiting its component; <see cref="For{TComponent}"/> renders it.</summary>
internal sealed class PendingNotification<T, TError, TViewModel>(
    Task<Result<T, TError>> result,
    TViewModel viewModel,
    Func<TError, ErrorContent> toContent)
    where TViewModel : class, new()
{
    internal Task<RazorComponentResult> For<TComponent>() where TComponent : INotifiableComponent<TViewModel>
    {
        return result.MatchToValue(
            _ => new[]
            {
                TComponent.AsRenderFragment(viewModel),
                Notifications.CreateNotificationToast(
                    TComponent.SuccessNotificationTitle,
                    TComponent.OkNotificationContent(viewModel))
            }.AggregateComponents(StatusCodes.Status200OK),
            error =>
            {
                var content = toContent(error);
                // A union's ToString prints only its type name.
                var details = (error is IUnion { Value: { } value } ? value : error)?.ToString();
                return new[]
                {
                    Notifications.CreateErrorNotification(TComponent.ErrorNotificationTitle, content, details)
                }.AggregateComponents(content.StatusCode);
            });
    }
}
