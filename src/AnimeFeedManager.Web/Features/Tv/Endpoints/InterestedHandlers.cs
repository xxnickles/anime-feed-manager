using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;
using AnimeFeedManager.Web.Features.Tv.Controls;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static partial class InterestedHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebTvSource);

    internal static async Task<RazorComponentResult> AddSeriesToInterested(
        [FromForm] TvInterestedViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<ForInterested> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Tv");
        return await Validate(viewModel)
            .Bind(model => Data.AddUser(context, model))
            .Bind(data => InterestedSeries.VerifyStorage(
                data.User,
                data.Model.SeriesId,
                data.Model.SeriesTitle,
                clientFactory.TableStorageTvSubscription, cancellationToken))
            .UpdateInterested(clientFactory.TableStorageTvSubscriptionUpdater,
                clientFactory.TableStorageTvSubscriptionsRemover, cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                // Render the ForInterestedRemoval component with a success notification
                _ =>
                [
                    ForInterestedRemoval.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast("Add Interested",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has been added to your interested list")),
                    Badge.AsOobFragment(StatusType.Secondary, "Interested", viewModel.CardBadgeId)
                ],
                error => [Notifications.CreateErrorToast("Add Interested", error)]);
    }


    internal static async Task<RazorComponentResult> RemoveInterestedSeries(
        [FromForm] TvInterestedViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<ForInterestedRemoval> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Tv");
        return await Validate(viewModel)
            .Bind(model => Data.AddUser(context, model))
            .Bind(data => InterestedSeries.VerifyStorage(
                data.User,
                data.Model.SeriesId,
                data.Model.SeriesTitle,
                clientFactory.TableStorageTvSubscription, cancellationToken))
            .UpdateInterested(clientFactory.TableStorageTvSubscriptionUpdater,
                clientFactory.TableStorageTvSubscriptionsRemover, cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                // Render the ForInterested component with a success notification
                _ =>
                [
                    ForInterested.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast("Remove Interested",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has been removed from your interested list")),
                    Badge.AsOobFragment(StatusType.Warning, "Not Available", viewModel.CardBadgeId)
                ],
                error => [Notifications.CreateErrorToast("Remove Interested", error)]);
    }
}