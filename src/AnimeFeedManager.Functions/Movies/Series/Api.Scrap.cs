using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Common.Domain.Validators;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Functions.ResponseExtensions;
using AnimeFeedManager.Functions.Scrapping;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Series;

public sealed class Scrap
{
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger _logger;

    public Scrap(
        IDomainPostman domainPostman,
        ILoggerFactory loggerFactory)
    {
        _domainPostman = domainPostman;
        _logger = loggerFactory.CreateLogger<Scrap>();
    }

    [Function("ScrapLatestMoviesSeason")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "movies/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Movies Library (Manual trigger)");

        var result = await req.AllowAdminOnly()
            .BindAsync(_ =>
                _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest)));

        // var result =
        //     await _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest));

        return await result.ToResponse(req, _logger);
    }

    [Function("ScrapCustomMoviesSeason")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "movies/library/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update Movies Library (Manual trigger) for Custom Season");

        var result = await req.AllowAdminOnly()
            .BindAsync(_ => SeasonValidators.Validate(season, year))
            .MapAsync(param => param.ToSeasonParameter())
            .BindAsync(param =>
                _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, param, ScrapType.BySeason)));

        // var result = await SeasonValidators.Validate(season, year)
        //     .Map(param => param.ToSeasonParameter())
        //     .BindAsync(param =>
        //         _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, param, ScrapType.BySeason)));

        return await result.ToResponse(req, _logger);
    }
}