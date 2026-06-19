using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Components;

/// <summary>
/// Builds out-of-band toast responses. An endpoint returns one of these while the trigger uses
/// <c>hx-swap="none"</c>, so the toast lands in <c>#toast-panel</c> without re-rendering the
/// triggering component. Reusable across admin / trigger actions; <see cref="Error"/> maps a
/// <see cref="DomainError"/> to a toast type + message.
/// </summary>
internal static class Notifications
{
    internal static RazorComponentResult Success(string title, RenderFragment message, TimeSpan? closeTime = null)
        => Toast(title, message, ToastType.Success, closeTime);

    internal static RazorComponentResult Error(string title, DomainError error, TimeSpan? closeTime = null)
        => Toast(title, ErrorContent(error), ToToastType(error), closeTime);

    internal static RazorComponentResult Toast(string title, RenderFragment message, ToastType type,
        TimeSpan? closeTime = null)
        => new RazorComponentResult<NotificationOob>(new Dictionary<string, object?>
        {
            [nameof(NotificationOob.Title)] = title,
            [nameof(NotificationOob.Message)] = message,
            [nameof(NotificationOob.Type)] = type,
            [nameof(NotificationOob.CloseTime)] = closeTime
        });

    internal static RenderFragment Text(string message) => builder => builder.AddContent(0, message);

    private static ToastType ToToastType(DomainError error) => error switch
    {
        NotFoundError => ToastType.Info,
        DomainValidationErrors or FormDataValidationError => ToastType.Warning,
        _ => ToastType.Error
    };

    private static RenderFragment ErrorContent(DomainError error) => error switch
    {
        NotFoundError => Text("The requested resource was not found."),
        DomainValidationErrors => Text("One or more validation errors occurred."),
        FormDataValidationError => Text("Form validation failed. Please check your input."),
        ExceptionError => Text("An internal error occurred. Please try again later."),
        _ => Text(error.Message)
    };
}
