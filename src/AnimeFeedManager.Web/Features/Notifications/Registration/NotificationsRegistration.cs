using AnimeFeedManager.Infrastructure.Sse;
using AnimeFeedManager.Web.Features.Security;

namespace AnimeFeedManager.Web.Features.Notifications.Registration;

/// <summary>
/// Composition root for SSE notifications. Registers the <see cref="SseBindings"/> registry as a
/// singleton, built once from each feature's own binding extension (<c>AddSecurityNotifications</c>,
/// …). <see cref="EventBus"/> is registered separately (<c>AddEventBus</c>); <see cref="SseStream"/>
/// is constructed per-connection in the endpoints so it receives the request-scoped provider the
/// HTML renderer needs — see <c>MapNotificationEndpoints</c>.
/// </summary>
internal static class NotificationsRegistration
{
    internal static IServiceCollection AddNotifications(this IServiceCollection services) =>
        services.AddSingleton(_ => new SseBindings()
            .AddSecurityNotifications());
}
