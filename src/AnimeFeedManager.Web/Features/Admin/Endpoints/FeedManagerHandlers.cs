using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Web.BlazorComponents.Toast;
using AnimeFeedManager.Web.Features.Admin.FeedManager;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class FeedManagerHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebAdminSource);

    internal static async Task<RazorComponentResult> Bind(
        [FromForm] BindReleaseViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<ReleaseCandidateRow> logger,
        CancellationToken token)
    {
        using var activity = Source.StartActivity(AdminTelemetry.BindNyaaSeriesSpan);
        return await Validate(viewModel)
            .Bind(vm => domainPostman.SendMessages(
                [new BindNyaaSeries(vm.SeriesId, vm.Group, vm.ReleaseTitle, vm.Variant ?? string.Empty, vm.Trusted)],
                token))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                _ =>
                [
                    Notifications.CreateNotificationToast("Feed binding",
                        Notifications.TextBody(
                            $"{viewModel.SeriesTitle} will be bound to [{viewModel.Group}] and indexed in the background."),
                        ToastType.Info)
                ],
                error => [Notifications.CreateErrorNotification("Feed binding", error)]);
    }
}
