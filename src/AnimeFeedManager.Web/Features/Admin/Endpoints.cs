using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Web.Features.Common;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
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
                    .ToComponentResult(renderer, logger, "Latest tv library will be scrapped in the background"));


        app.MapPut("/admin/tv/titles",
            ([FromForm] string? noop,
                    [FromServices] BlazorRenderer renderer,
                    [FromServices] IDomainPostman domainPostman,
                    [FromServices] ILogger<Admin> logger,
                    CancellationToken token) =>
                domainPostman.CreateScrapingEvent(new ScrapTvTilesRequest(), token: token)
                    .ToComponentResult(renderer, logger, "Tv Titles will be scrapped in the background"));


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
                            new ScrapLibraryRequest(SeriesType.Tv, seasonParameter, ScrapType.Latest),
                            token: token))
                    .ToComponentResult(renderer, logger,
                        $"Tv library for {season.Season}-{season.Year} will be scrapped in the background"));

        app.MapPut("/admin/ovas",
            ([FromForm] string? noop,
                    [FromServices] BlazorRenderer renderer,
                    [FromServices] IDomainPostman domainPostman,
                    [FromServices] ILogger<Admin> logger,
                    CancellationToken token) =>
                domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Ova, null, ScrapType.Latest),
                        token: token)
                    .ToComponentResult(renderer, logger, "Latest Ovas library will be scrapped in the background"));

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
                            new ScrapLibraryRequest(SeriesType.Ova, seasonParameter, ScrapType.Latest),
                            token: token))
                    .ToComponentResult(renderer, logger,
                        $"Ovas library for {season.Season}-{season.Year} will be scrapped in the background"));

        app.MapPut("/admin/movies",
            ([FromForm] string? noop,
                    [FromServices] BlazorRenderer renderer,
                    [FromServices] IDomainPostman domainPostman,
                    [FromServices] ILogger<Admin> logger,
                    CancellationToken token) =>
                domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest),
                        token: token)
                    .ToComponentResult(renderer, logger, "Latest movies library will be scrapped in the background"));

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
                            new ScrapLibraryRequest(SeriesType.Movie, seasonParameter, ScrapType.Latest),
                            token: token))
                    .ToComponentResult(renderer, logger,
                        $"Movies library for {season.Season}-{season.Year} will be scrapped in the background"));

        app.MapPut("/admin/noop", async (BlazorRenderer renderer) =>
        {
            var parameters = new Dictionary<string, object?>
            {
                {nameof(ErrorResult.Error), BasicError.Create("You have tried to do something that is not here")}
            };

            return Results.Content(await renderer.RenderComponent<ErrorResult>(parameters), "text/html");
        });
    }
}