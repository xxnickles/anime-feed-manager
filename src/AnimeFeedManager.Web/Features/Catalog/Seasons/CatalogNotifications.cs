using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Infrastructure.Sse;

namespace AnimeFeedManager.Web.Features.Catalog.Seasons;

/// <summary>
/// SSE bindings for catalog notifications. One <see cref="SeasonImported"/> occurrence renders as
/// two toasts at two audiences: a public "new season available" toast (everyone) and an admin
/// operational toast (import count). Admins receive both — an intentional double-delivery, since
/// <c>Audience ≤ level</c> is inclusive; it's the groundwork for a future persisted admin panel.
/// </summary>
internal static class CatalogNotifications
{
    internal static SseBindings AddCatalogNotifications(this SseBindings bindings) =>
        bindings
            .AddHtml<SeasonImported, SeasonImportedToast>(Audience.Public)
            .AddHtml<SeasonImported, SeasonImportedAdminToast>(Audience.Admin);
}
