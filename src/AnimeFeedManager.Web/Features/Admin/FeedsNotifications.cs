using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Infrastructure.Sse;

namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// SSE bindings for Feeds admin notifications. A <see cref="NyaaCollectionRunCompleted"/>
/// occurrence renders as a single admin-only operational toast — no public counterpart, unlike
/// <c>SeasonImported</c>'s double-delivery.
/// </summary>
internal static class FeedsNotifications
{
    internal static SseBindings AddFeedsNotifications(this SseBindings bindings) =>
        bindings.AddHtml<NyaaCollectionRunCompleted, NyaaCollectionRunCompletedToast>(Audience.Admin);
}
