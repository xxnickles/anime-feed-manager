using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Web.Features.Admin.Stats;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class StatHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebAdminSource);

    internal static async Task<RazorComponentResult> LastScrape(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.LastScrapeStatSpan);
        return await LastScrapeStat.Get(
                clientFactory.TableStorageEvents<ScrapTvLibraryResult>(),
                ChartDateRange.FromPeriod(period).From,
                cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                stat => [LastScrapeStatCard.AsRenderFragment(stat)],
                _ => [StatCardError.AsRenderFragment("Last scrape")]);
    }

    internal static async Task<RazorComponentResult> Errors(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.ErrorStatSpan);
        return await ErrorStat.Get(
                clientFactory.TableStorageErrorEvents(),
                ChartDateRange.FromPeriod(period).From,
                cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                stat => [ErrorsStatCard.AsRenderFragment(stat)],
                _ => [StatCardError.AsRenderFragment("Errors")]);
    }

    internal static async Task<RazorComponentResult> Nyaa(
        [FromQuery] string? period,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AdminPage> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.NyaaStatSpan);
        return await NyaaStat.Get(
                clientFactory.TableStorageEvents<NyaaSeriesBound>(),
                clientFactory.TableStorageEvents<NyaaSeriesBackfilled>(),
                ChartDateRange.FromPeriod(period).From,
                cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                stat => [NyaaStatCard.AsRenderFragment(stat)],
                _ => [StatCardError.AsRenderFragment("Nyaa")]);
    }
}
