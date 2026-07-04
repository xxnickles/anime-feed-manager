using AnimeFeedManager.Features.Auth.Events;
using AnimeFeedManager.Infrastructure.Sse;

namespace AnimeFeedManager.Web.Features.Security;

internal static class SecurityNotifications
{
    internal static SseBindings AddSecurityNotifications(this SseBindings bindings) =>
        bindings.AddHtml<UserRegistered, UserRegisteredNotification>(Audience.Admin);
}
