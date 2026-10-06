using AnimeFeedManager.Features.Tv.Library.Queries;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;
using AnimeFeedManager.Web.Features.Tv.Controls;
using Azure.Storage.Blobs;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static class SubscriptionHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebTvSource);

    internal static Task<RazorComponentResult> Subscribe(
        [FromForm] TvSeriesActionViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] BlobServiceClient blobServiceClient,
        [FromServices] ILogger<ForSubscription> logger,
        HttpContext context,
        CancellationToken cancellationToken) =>
        ToggleSubscription(viewModel, clientFactory, blobServiceClient.Uri, logger, context, cancellationToken);

    internal static Task<RazorComponentResult> Unsubscribe(
        [FromForm] TvSeriesActionViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] BlobServiceClient blobServiceClient,
        [FromServices] ILogger<ForSubscriptionRemoval> logger,
        HttpContext context,
        CancellationToken cancellationToken) =>
        ToggleSubscription(viewModel, clientFactory, blobServiceClient.Uri, logger, context, cancellationToken);

    // The store toggles from the persisted state, so the card and the message follow the resulting state.
    private static async Task<RazorComponentResult> ToggleSubscription(
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
            .Bind(data => Subscription.VerifyStorage(
                    data.User,
                    data.Series.Id,
                    data.Series.Title,
                    data.Series.FeedTitle ?? string.Empty,
                    data.Series.FeedUrl ?? string.Empty,
                    clientFactory.TableStorageTvSubscription, cancellationToken)
                .UpdateSubscription(clientFactory.TableStorageTvSubscriptionUpdater,
                    clientFactory.TableStorageTvSubscriptionsRemover, cancellationToken)
                .Map(state => data.Series.ForUser(data.User, state)))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                card =>
                [
                    TvCard.AsRenderFragment(card),
                    Notifications.CreateNotificationToast("TV Subscription",
                        Notifications.TextBody(card is Subscribed
                            ? $"{card.TvSeries.Title} has been added to your subscriptions"
                            : $"{card.TvSeries.Title} has been removed from your subscriptions"))
                ],
                error => [Notifications.CreateErrorNotification("TV Subscription", error)]);
    }
}
