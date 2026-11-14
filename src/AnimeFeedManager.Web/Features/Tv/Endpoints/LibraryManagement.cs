using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Web.Features.Tv.Controls;
using AnimeFeedManager.Web.Htmx.Static;
using static AnimeFeedManager.Features.Tv.Library.Management.Series;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static class LibraryManagement
{
    private static readonly ActivitySource Source = new(Telemetry.WebTvSource);

    internal static async Task<RazorComponentResult> RemoveSeries(
        [FromForm] RemoveSeriesViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<SeriesDeleter> logger,
        HttpContext context,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(TvTelemetry.RemoveSeriesSpan);
        return await Validate(viewModel)
            .MapError(error => (DomainError)error)
            .Bind(model => IdHelpers.SeriesSeasonFromId(model.SeriesId)
                .Map(season => (Model: model, Season: season)))
            .Bind(data => DeleteSeries(data.Model.SeriesId, data.Season,
                clientFactory.TableStorageTvSeriesRemover, token))
            .Map(result =>
            {
                context.Response.HxTrigger(
                    new RemoveSeriesTrigger(new RemoveSeriesEvent(viewModel.CardId)),
                    TvEndpointJsonContext.Default.RemoveSeriesTrigger);
                return result;
            })
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(_ =>
                [
                    Notifications.CreateNotificationToast("Remove Series",
                        Notifications.TextBody($"Series {viewModel.SeriesTitle} has been removed"))
                ],
                error => error.ToContentFallback(), "Remove Series");
    }
}