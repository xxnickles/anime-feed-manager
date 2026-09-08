using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Web.Features.Admin.SeasonCards;

namespace AnimeFeedManager.Web.Features.Admin.Endpoints;

internal static class SeasonAdminHandlers
{
    private static readonly ActivitySource Source = new(Telemetry.WebAdminSource);

    internal static async Task<RazorComponentResult> SetFeatured(
        [FromForm] FeaturedSeasonViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Web.Admin");
        return await Validate(viewModel)
            .Bind(vm => vm.Season.ParseAsSeriesSeason())
            .Map(season => season with {IsLatest = true})
            .Bind(season => domainPostman.SendMessages([new SeasonUpdated(season)], cancellationToken))
            .MarkActivityErroredOnError()
            .ToComponentNotification<Unit, FeaturedSeasonViewModel, FeaturedSeasonUpdate>(viewModel);
    }
}
