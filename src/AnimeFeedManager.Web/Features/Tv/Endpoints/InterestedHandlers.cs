using AnimeFeedManager.Features.Tv.Library.Queries;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;
using AnimeFeedManager.Web.Features.Tv.Controls;
using Azure.Storage.Blobs;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static class InterestedHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebTvSource);

    internal static Task<RazorComponentResult> AddSeriesToInterested(
        [FromForm] TvSeriesActionViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] BlobServiceClient blobServiceClient,
        [FromServices] ILogger<ForInterested> logger,
        HttpContext context,
        CancellationToken cancellationToken) =>
        ToggleInterested(viewModel, clientFactory, blobServiceClient.Uri, logger, context, cancellationToken);

    internal static Task<RazorComponentResult> RemoveInterestedSeries(
        [FromForm] TvSeriesActionViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] BlobServiceClient blobServiceClient,
        [FromServices] ILogger<ForInterestedRemoval> logger,
        HttpContext context,
        CancellationToken cancellationToken) =>
        ToggleInterested(viewModel, clientFactory, blobServiceClient.Uri, logger, context, cancellationToken);

    // The store toggles from the persisted state, so the card and the message follow the resulting state.
    private static async Task<RazorComponentResult> ToggleInterested(
        TvSeriesActionViewModel viewModel,
        ITableClientFactory clientFactory,
        Uri publicBlobUri,
        ILogger logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Tv");
        return await Validate(viewModel)
            .Bind(model => Data.GetSeriesForUser(context, model.SeriesId,
                clientFactory.TableStorageTvLibrarySeries, publicBlobUri, cancellationToken))
            .Bind(data => InterestedSeries.VerifyStorage(
                    data.User,
                    data.Series.Id,
                    data.Series.Title,
                    clientFactory.TableStorageTvSubscription, cancellationToken)
                .UpdateInterested(clientFactory.TableStorageTvSubscriptionUpdater,
                    clientFactory.TableStorageTvSubscriptionsRemover, cancellationToken)
                .Map(state => data.Series.ForUser(data.User, state)))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                card =>
                [
                    TvCard.AsRenderFragment(card),
                    Notifications.CreateNotificationToast("Interested",
                        Notifications.TextBody(card is Interested
                            ? $"{card.TvSeries.Title} has been added to your interested list"
                            : $"{card.TvSeries.Title} has been removed from your interested list"))
                ],
                error => [Notifications.CreateErrorNotification("Interested", error)]);
    }
}
