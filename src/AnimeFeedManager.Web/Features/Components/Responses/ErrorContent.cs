using AnimeFeedManager.Web.BlazorComponents.Toast;
using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Web.Features.Components.Responses;

/// <summary>What an error renders at the edge: a 5xx renders as a dialog, anything else as a toast of <see cref="Severity"/>.</summary>
internal sealed record ErrorContent(int StatusCode, ToastType Severity, RenderFragment Message)
{
    internal static ErrorContent Fault(string message) =>
        new(StatusCodes.Status500InternalServerError, ToastType.Error, Notifications.TextBody(message));

    internal static ErrorContent Unexpected { get; } = Fault("An unexpected error occurred.");
}
