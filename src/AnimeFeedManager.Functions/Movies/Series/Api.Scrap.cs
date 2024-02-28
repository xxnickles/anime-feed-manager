﻿using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Functions.ResponseExtensions;
using AnimeFeedManager.Functions.Scrapping;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Series;

public sealed class Scrap(
    IDomainPostman domainPostman,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Scrap>();

    [Function("ScrapLatestMoviesSeason")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "movies/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Movies Library (Manual trigger)");

        var result = await req.AllowAdminOnly()
            .BindAsync(_ =>
                domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest)));

        // var result =
        //     await _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, null, ScrapType.Latest));

        return await result.ToResponse(req, _logger);
    }

    [Function("ScrapCustomMoviesSeason")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "movies/library/{year:int}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update Movies Library (Manual trigger) for Custom Season");

        var result = await req.AllowAdminOnly()
            .BindAsync(_ => SeasonValidators.Parse(season, year))
            .MapAsync(param => param.ToSeasonParameter())
            .BindAsync(param =>
                domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, param, ScrapType.BySeason)));

        // var result = await SeasonValidators.Validate(season, year)
        //     .Map(param => param.ToSeasonParameter())
        //     .BindAsync(param =>
        //         _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Movie, param, ScrapType.BySeason)));

        return await result.ToResponse(req, _logger);
    }
}