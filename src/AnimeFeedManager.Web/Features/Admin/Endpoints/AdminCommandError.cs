using AnimeFeedManager.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>A validated admin form that queues domain messages.</summary>
internal union AdminCommandError(ValidationError, DeliveryError);

internal static class AdminCommandErrorContent
{
    extension(AdminCommandError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            DeliveryError delivery => delivery.ToContent()
        };
    }
}
