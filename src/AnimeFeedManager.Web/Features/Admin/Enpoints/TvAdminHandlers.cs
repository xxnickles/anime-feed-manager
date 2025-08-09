using AnimeFeedManager.Features.Infrastructure.Messaging;

namespace AnimeFeedManager.Web.Features.Admin.Enpoints;

internal class TvAdminHandlers
{
    internal static IResult BySeason(
        [FromForm] BySeasonViewModel viewModel,
        [FromServices] IDomainPostman domainPostman,
        CancellationToken cancellationToken)
    {
        return Results.Accepted();
    }
}