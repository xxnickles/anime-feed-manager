using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Web.Features.Admin.TvCards;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class TvAdminHandlers
{
    internal static Task<RazorComponentResult> BySeason(
        [FromForm] BySeasonViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(vm => domainPostman.SendMessages([new UpdateTvSeriesEvent(new SeasonParameters(vm.Season, vm.Year))],
                cancellationToken))
            .ToComponentNotification<Unit, BySeasonViewModel, TvSeasonalUpdate>(viewModel);
    }


    internal static Task<RazorComponentResult> Latest(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return domainPostman.SendMessages([new UpdateTvSeriesEvent()], cancellationToken)
            .ToComponentNotification<Unit, Noop, TvLatestSeasonUpdate>(new Noop());
    }

    internal static Task<RazorComponentResult> Titles(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return domainPostman.SendMessages([new UpdateLatestFeedTitlesEvent()], cancellationToken)
            .ToComponentNotification<Unit, Noop, TvTitlesUpdate>(new Noop());
    }
    
    internal static Task<RazorComponentResult> TriggerNotificationProcess(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return domainPostman.SendMessages([new RunFeedNotification()], cancellationToken)
            .ToComponentNotification<Unit, Noop, TvNotificationsTrigger>(new Noop());
    }
}