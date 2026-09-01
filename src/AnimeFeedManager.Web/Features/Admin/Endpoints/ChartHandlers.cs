using AnimeFeedManager.Features.SystemEvents.Charting;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Web.BlazorComponents.Charts;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class ChartHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebAdminSource);

    internal static async Task<RazorComponentResult> ScrapLibrarySummary(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        var range = ChartDateRange.FromPeriod(period);
        return await ScrapLibraryChart.Get(
                clientFactory.TableStorageEvents<ScrapTvLibraryResult>(),
                clientFactory.TableStorageEvents<ScrapTvLibraryFailedResult>(),
                range.From,
                range.To,
                cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Scraping Events", data, ChartJsOptions.IntegerScale)],
                error => [ChartError.AsRenderFragment(error.Message)]
            );
    }

    internal static async Task<RazorComponentResult> NotificationSummary(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        var range = ChartDateRange.FromPeriod(period);
        return await NotificationSentChart.Get(
                clientFactory.TableStorageBroadEvents<NotificationSent>(),
                range.From,
                range.To,
                cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Notifications Sent", data, ChartJsOptions.IntegerScale)],
                error => [ChartError.AsRenderFragment(error.Message)]);
    }

    internal static async Task<RazorComponentResult> FeedUpdatesSummary(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        var range = ChartDateRange.FromPeriod(period);
        return await FeedUpdatesChart.Get(
                clientFactory.TableStorageEvents<FeedTitlesUpdateResult>(),
                range.From,
                range.To,
                cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                data => [ChartContent.AsRenderFragment("Feed Updates", data, ChartJsOptions.IntegerScale)],
                error => [ChartError.AsRenderFragment(error.Message)]);
    }
}
