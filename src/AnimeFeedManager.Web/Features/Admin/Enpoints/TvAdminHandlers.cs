using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Web.Features.Admin.TvCards;
using AnimeFeedManager.Web.Features.Components.Responses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Admin.Enpoints;

internal class TvAdminHandlers
{
    internal static Task<RazorComponentResult> BySeason(
        [FromForm] BySeasonViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return Validate(viewModel)
            .Bind(vm => domainPostman.SendMessage(new UpdateTvSeriesEvent(new SeasonParameters(vm.Season, vm.Year)),
                cancellationToken))
            .ToComponentResult(
                _ =>
                [
                    Notifications.CreateNotificationToast(
                        "TV Season Updates",
                        TvSeasonalUpdate.OkNotificationContent(viewModel))
                ],
                error => [
                    TvSeasonalUpdate.AsRenderFragment(viewModel),
                    Notifications.CreateErrorToast("TV Season Updates failed", error)
                ]);
    }
}