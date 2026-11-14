using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Web.BlazorComponents.Toast;
using AnimeFeedManager.Web.Features.Tv.Endpoints;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class Notifications
{
    internal static RenderFragment CreateToast(string title, RenderFragment message, ToastType type,
        RenderFragment? actions = null, NotificationLifetime? lifetime = null) => builder =>
    {
        builder.OpenComponent<NotificationOob>(0);
        builder.AddComponentParameter(1, nameof(NotificationOob.Title), title);
        builder.AddComponentParameter(2, nameof(NotificationOob.Message), message);
        builder.AddComponentParameter(3, nameof(NotificationOob.Type), type);
        builder.AddComponentParameter(4, nameof(NotificationOob.Actions), actions);
        builder.AddComponentParameter(5, nameof(NotificationOob.Lifetime), lifetime ?? NotificationLifetime.Default);
        builder.CloseComponent();
    };

    internal static RenderFragment TextBody(string message) => builder => builder.AddContent(0, message);

    internal static RenderFragment CreateNotificationToast(string title, RenderFragment message,
        ToastType type = ToastType.Success) =>
        CreateToast(title, message, type);

    // 4xx errors are the user's to fix: a toast. 5xx are ours: a dialog with the trace ID.
    internal static RenderFragment CreateErrorNotification(string title, DomainError error) =>
        CreateErrorNotification(title, new ErrorContent(error.ToStatusCode(), ToToastType(error), ErrorToContent(error)),
            error.ToString());

    internal static RenderFragment CreateErrorNotification(string title, ErrorContent content, string? details) =>
        content.StatusCode >= StatusCodes.Status500InternalServerError
            ? CreateErrorDialog(title, content.Message, content.StatusCode, details)
            : CreateToast(title, content.Message, content.Severity);

    internal static RenderFragment CreateErrorDialog(string title, RenderFragment message, int statusCode,
        string? details) => builder =>
    {
        builder.OpenComponent<ErrorDialogOob>(0);
        builder.AddComponentParameter(1, nameof(ErrorDialogOob.Title), title);
        builder.AddComponentParameter(2, nameof(ErrorDialogOob.Message), message);
        builder.AddComponentParameter(3, nameof(ErrorDialogOob.StatusCode), statusCode);
        builder.AddComponentParameter(4, nameof(ErrorDialogOob.Details), details);
        builder.CloseComponent();
    };

    private static ToastType ToToastType(DomainError error) => error switch
    {
        StorageError.NotFound => ToastType.Info,
        ValidationError => ToastType.Warning,
        _ => ToastType.Error
    };

    private static RenderFragment ErrorToContent(DomainError error)
    {
        return builder =>
        {
            switch (error)
            {
                case StorageError.NotFound:
                    builder.AddContent(1, "The requested resource was not found.");
                    break;

                case ValidationError.Fields:
                    builder.AddContent(2, "One or more validation errors occurred:");
                    break;

                case StorageError.RequestFailed:
                    builder.AddContent(1, "An internal server error occurred. Please try again later.");
                    break;

                case NoFeed or AlreadySubscribed or UserIsAnonymous:
                    builder.AddContent(1, error.Message);
                    break;

                case ValidationError.Form:
                    builder.AddContent(1, "Form validation failed. Please check your input and try again.");
                    break;


                default:
                    builder.AddContent(1, "An unexpected error occurred.");
                    break;
            }
        };
    }
}