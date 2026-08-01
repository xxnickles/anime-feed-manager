using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Infrastructure.Sse;
using AnimeFeedManager.Web.Features.Admin.Activity;

namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// SSE bindings for Feeds admin notifications. <see cref="NyaaCollectionRunCompleted"/> and
/// <see cref="AiringClockCheckRunCompleted"/> render admin-only toasts (success-gated, no public
/// counterpart). A live <see cref="FeedsOccurrence"/> or <see cref="CollectionRun"/> also signals
/// <see cref="AdminActivityFeed"/> to refresh — <c>CollectionRun</c> unconditionally, on every
/// run, not just the ones that earn a toast. A named event carrying no data, so the client
/// re-GETs the feed rather than swapping in server-pushed HTML directly (see
/// <c>ActivityEndpoints</c>).
/// </summary>
internal static class FeedsNotifications
{
    internal static SseBindings AddFeedsNotifications(this SseBindings bindings) =>
        bindings
            .AddHtml<NyaaCollectionRunCompleted, NyaaCollectionRunCompletedToast>(Audience.Admin)
            .AddHtml<AiringClockCheckRunCompleted, AiringClockCheckRunCompletedToast>(Audience.Admin)
            .AddHtml<FeedsOccurrence, FeedsOccurrenceAdminToast>(Audience.Admin)
            .Add<FeedsOccurrence>(AdminActivityFeed.RefreshSseEvent, Audience.Admin, _ => string.Empty)
            .Add<CollectionRun>(AdminActivityFeed.RefreshSseEvent, Audience.Admin, _ => string.Empty);
}
