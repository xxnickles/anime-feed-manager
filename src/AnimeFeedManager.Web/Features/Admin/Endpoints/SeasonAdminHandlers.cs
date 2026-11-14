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
        [FromServices] ILogger<FeaturedSeasonUpdate> logger,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity(AdminTelemetry.SetFeaturedSeasonSpan);
        return await Validate(viewModel)
            .MapError(error => (AdminCommandError)error)
            .Bind(vm => vm.Season.ParseAsSeriesSeason().MapError(error => (AdminCommandError)error))
            .Map(season => season with {IsLatest = true})
            .Bind(season => domainPostman.SendMessages([new SeasonUpdated(season)], cancellationToken)
                .MapError(error => (AdminCommandError)error))
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentNotification(viewModel, error => error.ToContent())
            .For<FeaturedSeasonUpdate>();
    }
}
