using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Web.Features.Tv.Controls;
using static AnimeFeedManager.Features.Tv.Library.Management.AlternativeTitles;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static class LibraryManagement
{
    internal static Task<RazorComponentResult> UpdateAlternativeSeries(
        [FromForm] AlternativeTitlesViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<AlternativeTitlesEditor> logger,
        CancellationToken cancellationToken)
        => UpdateAlternativeTitles(
                viewModel.SeriesId,
                viewModel.Season,
                viewModel.AlternativeTitles ?? [],
                clientFactory.TableStorageTvSeriesGetter(),
                clientFactory.GetTvSeriesUpdater(),
                cancellationToken)
            .LogErrors(logger)
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

}