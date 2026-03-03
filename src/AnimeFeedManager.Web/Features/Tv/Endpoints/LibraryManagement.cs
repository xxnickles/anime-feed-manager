using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Web.Features.Tv.Controls;
using AnimeFeedManager.Web.Htmx.Static;
using static AnimeFeedManager.Features.Tv.Library.Management.Series;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static class LibraryManagement
{
    internal static Task<RazorComponentResult> UpdateAlternativeSeries(
        [FromForm] AlternativeTitlesViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AlternativeTitlesEditor> logger,
        CancellationToken cancellationToken)
        =>
            Validate(viewModel).Bind(model => UpdateAlternativeTitles(
                    model.SeriesId,
                    model.Season,
                    model.AlternativeTitles ?? [],
                    clientFactory.TableStorageTvSeriesGetter,
                    clientFactory.TableStorageTvSeriesUpdater,
                    cancellationToken))
                .FlushLogs(logger)
                .ToComponentResult(
                    _ =>
                    [
                        AlternativeTitlesEditor.AsRenderFragment(viewModel),
                        Notifications.CreateNotificationToast("Alternative Titles",
                            Notifications.TextBody($"Alternative titles for {viewModel.SeriesTitle} has been updated"))
                    ],
                    error =>
                    [
                        AlternativeTitlesEditor.AsRenderFragment(viewModel),
                        Notifications.CreateErrorToast("Alternative Titles", error)
                    ]);

    internal static Task<RazorComponentResult> RemoveSeries(
        [FromForm] RemoveSeriesViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<SeriesDeleter> logger,
        HttpContext context,
        CancellationToken token) =>
        Validate(viewModel)
            .Bind(model => DeleteSeries(viewModel.SeriesId, viewModel.Season,
                clientFactory.TableStorageTvSeriesRemover, token))
            .Map(result =>
            {
                context.Response.HxTriggerAfterSwap(
                    new RemoveSeriesTrigger(new RemoveSeriesEvent(viewModel.CardId)),
                    TvEndpointJsonContext.Default.RemoveSeriesTrigger);
                return result;
            })
            .FlushLogs(logger)
            .ToComponentResult(_ =>
                [
                    Notifications.CreateNotificationToast("Remove Series",
                        Notifications.TextBody($"Series {viewModel.SeriesTitle} has been removed"))
                ],
                error =>
                [
                    Notifications.CreateErrorToast("Alternative Titles", error)
                ]);
}