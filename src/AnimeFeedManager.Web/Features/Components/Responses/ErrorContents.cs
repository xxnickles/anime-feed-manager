using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Web.BlazorComponents.Toast;

namespace AnimeFeedManager.Web.Features.Components.Responses;

internal static class ErrorContents
{
    extension(ValidationError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError.Fields => new ErrorContent(StatusCodes.Status422UnprocessableEntity, ToastType.Warning,
                Notifications.TextBody("One or more validation errors occurred.")),
            ValidationError.Form => new ErrorContent(StatusCodes.Status422UnprocessableEntity, ToastType.Warning,
                Notifications.TextBody("Form validation failed. Please check your input and try again."))
        };
    }

    extension(DeliveryError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            DeliveryError.NotDelivered => ErrorContent.Fault("The request couldn't be queued. Please try again later."),
            DeliveryError.NoTargetBox => ErrorContent.Unexpected
        };
    }
}
