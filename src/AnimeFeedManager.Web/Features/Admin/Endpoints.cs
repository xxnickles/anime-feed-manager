using AnimeFeedManager.Old.Common;
using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Domain.Validators;
using AnimeFeedManager.Old.Common.Dto;
using AnimeFeedManager.Old.Features.Infrastructure.Messaging;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.Types;
using AnimeFeedManager.Old.Features.Users;
using AnimeFeedManager.Old.Features.Users.IO;
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
                ([FromForm] ShorSeriesLatestSeason payload,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Ova, null, ScrapType.Latest, payload.KeeepFeed),
                            token: token)
                        .ToComponentResult("Latest Ovas library will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/ovas/season",
                ([FromForm] ShortSeriesSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Parse(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Ova, seasonParameter, ScrapType.BySeason, season.KeeepFeed),
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

        group.MapPut("/admin/ovas/season-feed",
                ([FromForm] BasicSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.SendMessage(new ScrapOvasSeasonFeed(season), token)
                        .ToComponentResult(
                            $"Ovas feed for {season.Season}-{season.Year}  will be updated in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/movies",
                ([FromForm] ShorSeriesLatestSeason payload,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest, payload.KeeepFeed),
                            token: token)
                        .ToComponentResult("Latest movies library will be scrapped in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);

        group.MapPut("/admin/movies/season",
                ([FromForm] ShortSeriesSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    SeasonValidators.Parse(season.Season, season.Year)
                        .Map(param => param.ToSeasonParameter())
                        .BindAsync(seasonParameter =>
                            domainPostman.CreateScrapingEvent(
                                new ScrapLibraryRequest(SeriesType.Movie, seasonParameter, ScrapType.BySeason, season.KeeepFeed),
                                token: token))
                        .ToComponentResult(
                            $"Movies library for {season.Season}-{season.Year} will be scrapped in the background",
                            logger))
            .RequireAuthorization(Policies.AdminRequired);
        
        group.MapPut("/admin/movies/feed",
                ([FromForm] string? noop,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.SendMessage(new StartScrapMoviesFeed(FeedType.Complete), token)
                        .ToComponentResult("Movies feed will be updated in the background", logger))
            .RequireAuthorization(Policies.AdminRequired);
        
        group.MapPut("/admin/movies/season-feed",
                ([FromForm] BasicSeason season,
                        [FromServices] IDomainPostman domainPostman,
                        [FromServices] ILogger<Admin> logger,
                        CancellationToken token) =>
                    domainPostman.SendMessage(new ScrapMoviesSeasonFeed(season), token)
                        .ToComponentResult($"Movies feed for {season.Season}-{season.Year}  will be updated in the background", logger))
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