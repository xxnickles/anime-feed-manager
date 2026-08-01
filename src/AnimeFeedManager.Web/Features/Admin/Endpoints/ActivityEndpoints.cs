using AnimeFeedManager.Web.Features.Admin.Activity;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

/// <summary>
/// Read-only refresh for the Recent Activity feed. The SSE bindings for LibraryEvent /
/// FeedsOccurrence / CollectionRun only signal "something changed" (see
/// NotificationsRegistration) — the client reacts by GETting this endpoint and swapping in a
/// freshly rendered <see cref="AdminActivityFeed"/>. Nests under the caller's shared /admin
/// group (auth applied once there) rather than building its own.
/// </summary>
internal static class ActivityEndpoints
{
    internal static IEndpointRouteBuilder MapActivityEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/activity", () => new RazorComponentResult<AdminActivityFeed>());
        return routes;
    }
}
