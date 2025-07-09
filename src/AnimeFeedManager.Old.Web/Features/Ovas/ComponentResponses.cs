using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Web.Features.Ovas.Controls;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Old.Web.Features.Ovas;

public static class ComponentResponses
{
    internal static RazorComponentResult OkSubscribedResponse(OvaControlData data, string message)

    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(SubscribedOvaControls.ControlData), data},
            {nameof(SubscribedOvaControls.Message), message}
        };
        return new RazorComponentResult<SubscribedOvaControls>(parameters);
    }
    
    internal static RazorComponentResult OkUnsubscribedResponse(OvaControlData data, string message)

    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UnsubscribedOvaControls.ControlData), data},
            {nameof(UnsubscribedOvaControls.Message), message}
        };
        return new RazorComponentResult<UnsubscribedOvaControls>(parameters);
    }

    internal static RazorComponentResult ErrorResponse(OvaControlData data, DomainError domainError,
        ILogger logger)
    {
        domainError.LogError(logger);
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UnsubscribedOvaControls.ControlData), data},
            {nameof(UnsubscribedOvaControls.DomainError), domainError}
        };
        return new RazorComponentResult<UnsubscribedOvaControls>(parameters);
    }
    
    internal static RazorComponentResult OkResponse(SeasonInformation seasonInformation, string message)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(OvasGridComponent.SeasonInfo), seasonInformation},
            {nameof(OvasGridComponent.Message), message}
        };
        return new RazorComponentResult<OvasGridComponent>(parameters);
    }
}