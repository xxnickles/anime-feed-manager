using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Web.Features.Admin.TvCards;
using AnimeFeedManager.Web.Features.Components.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class TvAdminHandlers
{
    internal static Task<RazorComponentResult> BySeason(
        [FromForm] BySeasonViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(vm => domainPostman.SendMessage(new UpdateTvSeriesEvent(new SeasonParameters(vm.Season, vm.Year)),
                cancellationToken))
            .ToComponentNotification<Unit, BySeasonViewModel, TvSeasonalUpdate>(viewModel);
    }


    internal static Task<RazorComponentResult> Latest(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return domainPostman.SendMessage(new UpdateTvSeriesEvent(), cancellationToken)
            .ToComponentNotification<Unit, Noop, TvLatestSeasonUpdate>(new Noop());
    }

    internal static Task<RazorComponentResult> Titles(
        [FromForm] Noop noop,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return domainPostman.SendMessage(new UpdateLatestFeedTitlesEvent(), cancellationToken)
            .ToComponentNotification<Unit, Noop, TvTitlesUpdate>(new Noop());
    }
}