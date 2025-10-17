using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Web.Features.Tv.Controls;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal static class SubscriptionHandlers
{
    internal static Task<RazorComponentResult> Subscribe(
        [FromForm] TvSubscriptionViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<ForSubscription> logger,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(model => Subscription.VerifyStorage(
                model.UserId,
                model.SeriesId, 
                model.SeriesTitle,
                model.SeriesFeedTitle,
                clientFactory.TableStorageTvSubscription(), cancellationToken))
            .UpdateSubscription(clientFactory.TableStorageTvSubscriptionUpdater(),
                clientFactory.TableStorageTvSubscriptionsRemover(), cancellationToken)
            .LogErrors(logger)
            .ToComponentResult(
                // Render the ForSubscriptionRemoval component with a success notification
                _ =>
                [
                    ForSubscriptionRemoval.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast("TV Subscription",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has been added to your subscriptions")),
                    Badge.AsOobFragment(StatusType.Primary, "Subscribed", viewModel.CardBadgeId)
                ],
                // Render the ForSubscription component again with an error notification
                error =>
                [
                    ForSubscription.AsRenderFragment(viewModel),
                    Notifications.CreateErrorToast("TV Subscription", error)
                ]);
    }


    internal static Task<RazorComponentResult> Unsubscribe(
        [FromForm] TvSubscriptionViewModel viewModel,
        [FromServices] ITableClientFactory clientFactory,
        [FromServices] ILogger<ForSubscriptionRemoval> logger,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(model => Subscription.VerifyStorage(
                model.UserId,
                model.SeriesId,
                model.SeriesTitle,
                model.SeriesFeedTitle,
                clientFactory.TableStorageTvSubscription(), cancellationToken))
            .UpdateSubscription(clientFactory.TableStorageTvSubscriptionUpdater(),
                clientFactory.TableStorageTvSubscriptionsRemover(), cancellationToken)
            .LogErrors(logger)
            .ToComponentResult(
                // Render the ForSubscription component with a success notification
                _ =>
                [
                    ForSubscription.AsRenderFragment(viewModel),
                    Notifications.CreateNotificationToast("Unsubscribe",
                        Notifications.TextBody($"{viewModel.SeriesTitle} has been removed from your subscriptions")),
                    Badge.AsOobFragment(StatusType.Success, "Available", viewModel.CardBadgeId)
                ],
                // Render the ForSubscriptionRemoval component again with an error notification
                error =>
                [
                    ForSubscriptionRemoval.AsRenderFragment(viewModel),
                    Notifications.CreateErrorToast("Unsubscribe", error)
                ]);
    }
}