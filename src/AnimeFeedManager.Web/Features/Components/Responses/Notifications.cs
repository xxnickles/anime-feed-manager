using AnimeFeedManager.Web.BlazorComponents.Toast;
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
        error.ToStatusCode() >= StatusCodes.Status500InternalServerError
            ? CreateErrorDialog(title, ErrorToContent(error), error.ToStatusCode(), error.ToString())
            : CreateToast(title, ErrorToContent(error), ToToastType(error));

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
        NotFoundError => ToastType.Info,
        DomainValidationErrors or FormDataValidationError => ToastType.Warning,
        _ => ToastType.Error
    };

    private static RenderFragment ErrorToContent(DomainError error)
    {
        return builder =>
        {
            switch (error)
            {
                case NotFoundError:
                    builder.AddContent(1, "The requested resource was not found.");
                    break;

                case DomainValidationErrors:
                    builder.AddContent(2, "One or more validation errors occurred:");
                    break;

                case ExceptionError:
                    builder.AddContent(1, "An internal server error occurred. Please try again later.");
                    break;

                case Error basic:
                    builder.AddContent(1, basic.Message);
                    break;

                case FormDataValidationError:
                    builder.AddContent(1, "Form validation failed. Please check your input and try again.");
                    break;


                default:
                    builder.AddContent(1, "An unexpected error occurred.");
                    break;
            }
        };
    }
}