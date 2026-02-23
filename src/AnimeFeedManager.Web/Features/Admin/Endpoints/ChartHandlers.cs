using AnimeFeedManager.Features.SystemEvents.Charting;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Web.BlazorComponents.Charts;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class ChartHandlers
{
    internal static Task<RazorComponentResult> ScrapLibrarySummary(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        var range = ChartDateRange.FromPeriod(period);
        return ScrapLibraryChart.Get(
                clientFactory.TableStorageEvents<ScrapTvLibraryResult>(),
                range.From,
                range.To,
                cancellationToken)
            .FlushLogs(logger)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Scraping Events", data, ChartJsOptions.IntegerScale)],
                error => [ChartError.AsRenderFragment(error.Message)]
            );
    }

    internal static Task<RazorComponentResult> NotificationSummary(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        var range = ChartDateRange.FromPeriod(period);
        return NotificationSentChart.Get(
                clientFactory.TableStorageBroadEvents<NotificationSent>(),
                range.From,
                range.To,
                cancellationToken)
            .FlushLogs(logger)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Notifications Sent", data, ChartJsOptions.IntegerScale)],
                error => [ChartError.AsRenderFragment(error.Message)]);
    }
    
    internal static Task<RazorComponentResult> FeedUpdatesSummary(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        var range = ChartDateRange.FromPeriod(period);
        return FeedUpdatesChart.Get(
                clientFactory.TableStorageEvents<FeedTitlesUpdateResult>(),
                range.From,
                range.To,
                cancellationToken)
            .FlushLogs(logger)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Feed Updates", data, ChartJsOptions.IntegerScale)],
                error => [ChartError.AsRenderFragment(error.Message)]);
    }
}
