using AnimeFeedManager.Common;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Validators;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Functions.ResponseExtensions;
using AnimeFeedManager.Functions.Scrapping;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Series;

public sealed class Scrap(
    IDomainPostman domainPostman,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Scrap>();

    [Function("ScrapLatestTvSeason")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Library (Manual trigger)");

        var result = await req.AllowAdminOnly()
            .BindAsync(_ =>
                domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Tv, null, ScrapType.Latest)));

        // var result =
        //     await _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Tv, null, ScrapType.Latest));
        
        return await result.ToResponse(req, _logger);
    }
    
    [Function("ScrapCustomTvSeason")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/library/{year:int}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update Library (Manual trigger) for Custom Season");
        
        var result = await req.AllowAdminOnly()
            .BindAsync(_ => SeasonValidators.Parse(season, year))
            .MapAsync(param => param.ToSeasonParameter())
            .BindAsync(param =>
                domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Tv, param, ScrapType.BySeason)));

        // var result = await SeasonValidators.Validate(season, year)
        //     .Map(param => param.ToSeasonParameter())
        //     .BindAsync(param =>
        //         _domainPostman.CreateScrapingEvent(new ScrapLibraryRequest(SeriesType.Tv, param, ScrapType.BySeason)));

        return await result.ToResponse(req, _logger);

    }
}