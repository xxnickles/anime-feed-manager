using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Features.Users;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Admin;

public static class Endpoints
{
    public static void MapAdminEndpoints(this RouteGroupBuilder group)
    {
        group.MapPut("/admin/tv",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Tv, null, ScrapType.Latest),
                            token: token)
                        .ToComponentResult("Latest tv library will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/tv/titles",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapTvTilesRequest(), token: token)
                        .ToComponentResult("Tv Titles will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/tv/season",
                ([FromForm] BasicSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Parse(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Tv, seasonParameter, ScrapType.BySeason),
                                token: token))
                        .ToComponentResult(
                            $"Tv library for {season.Season}-{season.Year} will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/ovas",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Ova, null, ScrapType.Latest),
                            token: token)
                        .ToComponentResult("Latest Ovas library will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/ovas/season",
                ([FromForm] BasicSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Parse(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Ova, seasonParameter, ScrapType.BySeason),
                                token: token))
                        .ToComponentResult(
                            $"Ovas library for {season.Season}-{season.Year} will be scrapped in the background",
                            logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/ovas/feed",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.SendMessage(new StartScrapOvasFeed(FeedType.Complete), token)
                        .ToComponentResult("Ovas feed will be updated in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/movies",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest),
                            token: token)
                        .ToComponentResult("Latest movies library will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/movies/season",
                ([FromForm] BasicSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Parse(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Movie, seasonParameter, ScrapType.BySeason),
                                token: token))
                        .ToComponentResult(
                            $"Movies library for {season.Season}-{season.Year} will be scrapped in the background",
                            logger))
            .RequireAuthorization(Policies.AdminRequired);


        group.MapPut("/admin/seasons",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.SendMessage(new UpdateLatestSeasonsRequest(), token)
                        .ToComponentResult("Latest Titles will be processed in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("admin/user/copy",
            ([FromForm] CopyUserPayload payload,
                [FromServices] SubscriptionCopierSetter subscriptionsCopier,
                [FromServices] ILogger<Admin> logger,
                CancellationToken token) => payload.Parse()
                .BindAsync(data => subscriptionsCopier.StartCopyProcess(data.Source, data.Target, token))
                .ToComponentResult("Copy of subscription will be processed in the background", logger)
        ).RequireAuthorization(Policies.AdminRequired);

        group.MapPut("admin/user/delete",
            ([FromForm] string source,
                    [FromServices] IUserDelete userDeleter,
                    [FromServices] ILogger<Admin> logger,
                    CancellationToken token) =>
                UserId.Parse(source)
                    .BindAsync(userId => userDeleter.Delete(userId, token))
                    .ToComponentResult(
                        "User has been deleted from the system. Subscriptions will be processed in the background",
                        logger)
        ).RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/noop",
            ([FromServices] ILogger<Admin> logger) => Task.FromResult(
                CommonComponentResponses.ErrorComponentResult(
                    BasicError.Create("You have tried to do something that is not here"), logger)));
    }
}