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
}
