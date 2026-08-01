using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Sse;
using AnimeFeedManager.Web.Features.Admin.Activity;

namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// SSE bindings for Library admin notifications. A live <see cref="LibraryEvent"/> renders an
/// admin-only toast and, separately, signals <see cref="AdminActivityFeed"/> to refresh — a
/// named event carrying no data, so the client re-GETs the feed rather than swapping in
/// server-pushed HTML directly (see <c>ActivityEndpoints</c>).
/// </summary>
internal static class LibraryNotifications
{
    internal static SseBindings AddLibraryNotifications(this SseBindings bindings) =>
        bindings
            .AddHtml<LibraryEvent, LibraryEventAdminToast>(Audience.Admin)
            .Add<LibraryEvent>(AdminActivityFeed.RefreshSseEvent, Audience.Admin, _ => string.Empty);
}
