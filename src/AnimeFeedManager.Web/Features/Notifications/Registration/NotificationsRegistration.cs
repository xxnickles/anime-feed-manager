using AnimeFeedManager.Infrastructure.Eventing;
using AnimeFeedManager.Infrastructure.Sse;
using AnimeFeedManager.Web.Features.Admin;
using AnimeFeedManager.Web.Features.Catalog.Seasons;
using AnimeFeedManager.Web.Features.Components;
using AnimeFeedManager.Web.Features.Security;

namespace AnimeFeedManager.Web.Features.Notifications.Registration;

/// <summary>
/// Composition root for SSE notifications. Registers the <see cref="SseBindings"/> registry as a
/// singleton, built once from each feature's own binding extension (<c>AddSecurityNotifications</c>,
/// …) plus the one cross-cutting <see cref="OperationFailed"/> binding — not feature-owned, so it's
/// registered here directly rather than via a per-feature extension. <see cref="EventBus"/> is
/// registered separately (<c>AddEventBus</c>); <see cref="SseStream"/> is constructed per-connection
/// in the endpoints so it receives the request-scoped provider the HTML renderer needs — see
/// <c>MapNotificationEndpoints</c>.
/// </summary>
internal static class NotificationsRegistration
{
    internal static IServiceCollection AddNotifications(this IServiceCollection services) =>
        services.AddSingleton(_ => new SseBindings()
            .AddSecurityNotifications()
            .AddCatalogNotifications()
            .AddLibraryNotifications()
            .AddFeedsNotifications()
            .AddHtml<OperationFailed, OperationFailedToast>(Audience.Admin));
}
