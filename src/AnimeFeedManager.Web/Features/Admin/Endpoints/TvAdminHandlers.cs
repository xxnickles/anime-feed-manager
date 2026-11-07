using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Web.Features.Admin.TvCards;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class TvAdminHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebAdminSource);

    internal static async Task<RazorComponentResult> BySeason(
        [FromForm] BySeasonViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<TvSeasonalUpdate> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.UpdateTvSeasonSpan);
        return await Validate(viewModel)
            .Bind(vm => domainPostman.SendMessages([new UpdateTvSeriesEvent(new SeasonParameters(vm.Season, vm.Year))],
                cancellationToken))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentNotification<Unit, BySeasonViewModel, TvSeasonalUpdate>(viewModel);
    }


    internal static async Task<RazorComponentResult> Latest(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<TvLatestSeasonUpdate> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.UpdateLatestTvSeasonSpan);
        return await domainPostman.SendMessages([new UpdateTvSeriesEvent()], cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentNotification<Unit, Noop, TvLatestSeasonUpdate>(new Noop());
    }

    internal static async Task<RazorComponentResult> TriggerNotificationProcess(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<TvNotificationsTrigger> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.TriggerFeedNotificationsSpan);
        return await domainPostman.SendMessages([new RunFeedNotification()], cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentNotification<Unit, Noop, TvNotificationsTrigger>(new Noop());
    }

    internal static async Task<RazorComponentResult> TriggerAutomatedBinding(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<AutomatedBindingTrigger> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.TriggerAutomatedBindingSpan);
        return await domainPostman.SendMessages([new RunAutomatedBinding()], cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentNotification<Unit, Noop, AutomatedBindingTrigger>(new Noop());
    }

    internal static async Task<RazorComponentResult> TriggerFeedSettlement(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        [FromServices] ILogger<FeedSettlementTrigger> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.TriggerFeedSettlementSpan);
        return await domainPostman.SendMessages([new RunFeedSettlement()], cancellationToken)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentNotification<Unit, Noop, FeedSettlementTrigger>(new Noop());
    }
}
