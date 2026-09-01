using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
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
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        return await Validate(viewModel)
            .Bind(vm => domainPostman.SendMessages([new UpdateTvSeriesEvent(new SeasonParameters(vm.Season, vm.Year))],
                cancellationToken))
            .MarkActivityErroredOnError()
            .ToComponentNotification<Unit, BySeasonViewModel, TvSeasonalUpdate>(viewModel);
    }


    internal static async Task<RazorComponentResult> Latest(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        return await domainPostman.SendMessages([new UpdateTvSeriesEvent()], cancellationToken)
            .MarkActivityErroredOnError()
            .ToComponentNotification<Unit, Noop, TvLatestSeasonUpdate>(new Noop());
    }

    internal static async Task<RazorComponentResult> Titles(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        return await domainPostman.SendMessages([new UpdateLatestFeedTitlesEvent()], cancellationToken)
            .MarkActivityErroredOnError()
            .ToComponentNotification<Unit, Noop, TvTitlesUpdate>(new Noop());
    }

    internal static async Task<RazorComponentResult> TriggerNotificationProcess(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        return await domainPostman.SendMessages([new RunFeedNotification()], cancellationToken)
            .MarkActivityErroredOnError()
            .ToComponentNotification<Unit, Noop, TvNotificationsTrigger>(new Noop());
    }
}