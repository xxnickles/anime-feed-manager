using AnimeFeedManager.Infrastructure.Eventing;
using AnimeFeedManager.Infrastructure.Sse;
using AnimeFeedManager.Web.Features.Security;

namespace AnimeFeedManager.Web.Features.Notifications.Endpoints;

/// <summary>
/// One SSE endpoint per audience. Each connection is a long-lived HTTP request, so ASP.NET Core
/// hands the handler a request-scoped <see cref="IServiceProvider"/> (its <c>RequestServices</c>)
/// that stays alive for exactly as long as the stream is open — that's the provider the HTML
/// bindings render with, threaded through <see cref="SseStream"/>. The route's fixed
/// <see cref="Audience"/> is the connection's level; <see cref="SseStream"/> subscribes only
/// bindings at or below it, so <c>/public ⊆ /registered ⊆ /admin</c> falls out of the filter.
/// A viewer opens exactly one of these, at their max entitled audience.
/// </summary>
internal static class NotificationEndpoints
{
    internal static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var sse = routes.MapGroup("/sse");

        sse.MapGet("/public", (IServiceProvider services, EventBus eventBus, SseBindings bindings, CancellationToken cancellationToken) =>
            Stream(services, eventBus, bindings, Audience.Public, cancellationToken));

        sse.MapGet("/registered", (IServiceProvider services, EventBus eventBus, SseBindings bindings, CancellationToken cancellationToken) =>
                Stream(services, eventBus, bindings, Audience.Registered, cancellationToken))
            .RequireAuthorization();

        sse.MapGet("/admin", (IServiceProvider services, EventBus eventBus, SseBindings bindings, CancellationToken cancellationToken) =>
                Stream(services, eventBus, bindings, Audience.Admin, cancellationToken))
            .RequireAuthorization(Policies.AdminRequired);

        return routes;
    }

    private static IResult Stream(
        IServiceProvider services, EventBus eventBus, SseBindings bindings, Audience level, CancellationToken cancellationToken) =>
        TypedResults.ServerSentEvents(new SseStream(eventBus, bindings, services, level).Stream(cancellationToken));
}
