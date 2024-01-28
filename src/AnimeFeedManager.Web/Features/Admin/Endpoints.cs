using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Users;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Admin;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPut("/admin/tv",
                ([FromForm] string? noop,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Tv, null, ScrapType.Latest),
                            token: token)
                        .ToComponentResult(renderer, logger, "Latest tv library will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/tv/titles",
                ([FromForm] string? noop,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapTvTilesRequest(), token: token)
                        .ToComponentResult(renderer, logger, "Tv Titles will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/tv/season",
                ([FromForm] BasicSeason season,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Validate(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Tv, seasonParameter, ScrapType.BySeason),
                                token: token))
                        .ToComponentResult(renderer, logger,
                            $"Tv library for {season.Season}-{season.Year} will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/ovas",
                ([FromForm] string? noop,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Ova, null, ScrapType.Latest),
                            token: token)
                        .ToComponentResult(renderer, logger, "Latest Ovas library will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/ovas/season",
                ([FromForm] BasicSeason season,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Validate(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Ova, seasonParameter, ScrapType.BySeason),
                                token: token))
                        .ToComponentResult(renderer, logger,
                            $"Ovas library for {season.Season}-{season.Year} will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/movies",
                ([FromForm] string? noop,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest),
                            token: token)
                        .ToComponentResult(renderer, logger,
                            "Latest movies library will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/movies/season",
                ([FromForm] BasicSeason season,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Validate(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Movie, seasonParameter, ScrapType.BySeason),
                                token: token))
                        .ToComponentResult(renderer, logger,
                            $"Movies library for {season.Season}-{season.Year} will be scrapped in the background"))
            .RequireAuthorization(Policies.AdminRequired);


        app.MapPut("/admin/seasons",
                ([FromForm] string? noop,
                        [FromServices] BlazorRenderer renderer,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.SendMessage(new UpdateLatestSeasonsRequest(), Box.LatestSeason, token)
                        .ToComponentResult(renderer, logger,
                            "Latest Titles will be processed in the background"))
            .RequireAuthorization(Policies.AdminRequired);

        app.MapPut("admin/user/copy",
            ([FromForm] CopyUserPayload payload,
                [FromServices] BlazorRenderer renderer,
                [FromServices] SubscriptionCopierSetter subscriptionsCopier,
                [FromServices] ILogger<Admin> logger,
                CancellationToken token) => payload.Parse()
                .BindAsync(data => subscriptionsCopier.StartCopyProcess(data.Source, data.Target, token))
                .ToComponentResult(renderer, logger, "Copy of subscription will be processed in the background")
        ).RequireAuthorization(Policies.AdminRequired);

        app.MapPut("admin/user/delete",
            ([FromForm] string source,
                [FromServices] BlazorRenderer renderer,
                [FromServices] IUserDelete userDeleter,
                [FromServices] ILogger<Admin> logger,
                CancellationToken token) => UserIdValidator.Validate(source)
                .ValidationToEither()
                .BindAsync(userId => userDeleter.Delete(userId, token))
                .ToComponentResult(renderer, logger, "User has been deleted from the system. Subscriptions will be processed in the background")
        ).RequireAuthorization(Policies.AdminRequired);

        app.MapPut("/admin/noop", async (BlazorRenderer renderer) =>
        {
            var parameters = new Dictionary<string, object?>
            {
                {nameof(ErrorResult.Error), BasicError.Create("You have tried to do something that is not here")}
            };
            var content = await renderer.RenderComponent<ErrorResult>(parameters);
            return Results.Content(content.Html, "text/html");
        });
    }
}