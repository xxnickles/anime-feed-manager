using AnimeFeedManager.Web.BlazorComponents.Toast;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class Notifications
{
    internal static RenderFragment CreateToast(Notification notification)
    {
        return builder =>
        {
            // Outer div with hx-swap-oob attribute
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "hx-swap-oob", "afterbegin:#toast-panel");

            // Inner ClosableNotification component
            builder.OpenComponent<ClosableNotification>(2);
            builder.AddAttribute(3, "Title", notification.Title);
            builder.AddAttribute(4, "Message", notification.Message);
            builder.AddAttribute(5, "Type", notification.Type);
            builder.AddAttribute(6, "CloseTime", notification.CloseTime ?? TimeSpan.FromSeconds(8));
            builder.CloseComponent();

            // Close outer div
            builder.CloseElement();
        };
    }
    
    internal static RenderFragment CreateNotificationToast(string title, RenderFragment message, ToastType type = ToastType.Success) =>
        CreateToast(new Notification(title, message, type));

    internal static RenderFragment CreateErrorToast(string title, DomainError error) =>
        CreateToast(new Notification(title, ErrorToContent(error), ToToastType(error)));

    private static ToastType ToToastType(DomainError error) => error switch
    {
        NotFoundError => ToastType.Info,
        DomainValidationErrors => ToastType.Warning,
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

                case OperationError op:
                    builder.OpenElement(1, "strong");
                    builder.AddContent(2, $"Operation '{op.Operation}' failed");
                    builder.CloseElement();
                    builder.AddContent(3, $": {op.Message}");
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