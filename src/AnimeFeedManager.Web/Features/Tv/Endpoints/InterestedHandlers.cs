using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Features.Tv.Controls;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static partial class InterestedHandlers
{
    internal static Task<RazorComponentResult> AddSeriesToInterested(
        [FromForm] TvInterestedViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<ForInterested> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(model => Data.AddUser(context, model))
            .Bind(data => InterestedSeries.VerifyStorage(
                data.User,
                data.Model.SeriesId,
                data.Model.SeriesTitle,
                clientFactory.TableStorageTvSubscription, cancellationToken))
            .UpdateInterested(clientFactory.TableStorageTvSubscriptionUpdater,
                clientFactory.TableStorageTvSubscriptionsRemover, cancellationToken)
            .LogErrors(logger)
            .ToComponentResult(
                // Render the ForInterestedRemoval component with a success notification
                _ =>
                [
                    ForInterestedRemoval.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast("Add Interested",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has been added to your interested list")),
                    Badge.AsOobFragment(StatusType.Secondary, "Interested", viewModel.CardBadgeId)
                ],
                // Render the ForInterested component again with an error notification
                error =>
                [
                    ForInterested.AsRenderFragment(viewModel),
                    Notifications.CreateErrorToast("Add Interested", error)
                ]);
    }


    internal static Task<RazorComponentResult> RemoveInterestedSeries(
        [FromForm] TvInterestedViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<ForInterestedRemoval> logger,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(model => Data.AddUser(context, model))
            .Bind(data => InterestedSeries.VerifyStorage(
                data.User,
                data.Model.SeriesId,
                data.Model.SeriesTitle,
                clientFactory.TableStorageTvSubscription, cancellationToken))
            .UpdateInterested(clientFactory.TableStorageTvSubscriptionUpdater,
                clientFactory.TableStorageTvSubscriptionsRemover, cancellationToken)
            .LogErrors(logger)
            .ToComponentResult(
                // Render the ForInterested component with a success notification
                _ =>
                [
                    ForInterested.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast("Remove Interested",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has been removed from your interested list")),
                    Badge.AsOobFragment(StatusType.Warning, "Not Available", viewModel.CardBadgeId)
                ],
                // Render the ForInterestedRemoval component again with an error notification
                error =>
                [
                    ForInterestedRemoval.AsRenderFragment(viewModel),
                    Notifications.CreateErrorToast("Remove Interested", error)
                ]);
    }
}