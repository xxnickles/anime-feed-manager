using AnimeFeedManager.Features.SystemEvents.Reporting;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Web.BlazorComponents.Charts;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class ChartHandlers
{
    internal static Task<RazorComponentResult> ScrapLibrarySummary(
        [FromServices] ITableClientFactory clientFactory,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return ScrapLibraryChart.Get(
                clientFactory.TableStorageEvents<ScrapTvLibraryResult>(),
                now.AddMonths(-1),
                now, cancellationToken)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Scraps", data)],
                error => [ChartError.AsRenderFragment()]
            );
    }
}