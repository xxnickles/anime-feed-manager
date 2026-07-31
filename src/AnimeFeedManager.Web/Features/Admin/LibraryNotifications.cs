using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Sse;

namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// SSE bindings for Library admin notifications. A live <see cref="LibraryEvent"/> renders as a
/// single admin-only operational toast — no public counterpart.
/// </summary>
internal static class LibraryNotifications
{
    internal static SseBindings AddLibraryNotifications(this SseBindings bindings) =>
        bindings.AddHtml<LibraryEvent, LibraryEventAdminToast>(Audience.Admin);
}
