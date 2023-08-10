using AnimeFeedManager.Backend.Functions.ResponseExtensions;
using AnimeFeedManager.Features.Common.Dto;
using AnimeFeedManager.Features.Movies.Scrapping;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Movies;

public class Scrap
{
    private readonly MoviesLibraryUpdater _libraryUpdater;
    private readonly ILogger _logger;
    
    public Scrap(
        MoviesLibraryUpdater libraryUpdater,
        ILoggerFactory loggerFactory)
    {
        _libraryUpdater = libraryUpdater;
        _logger = loggerFactory.CreateLogger<Scrap>();
    }

    [Function("ScrapLatestMoviesSeason")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "movies/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Movies Library (Manual trigger)");

        var result = await _libraryUpdater.Update(new Latest());
        
        return await result.Match(
            _ => req.Ok(),
            error => error.ToResponse(req, _logger)
        );
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
            .BindAsync(_ => _libraryUpdater.Update(new BySeason(new SimpleSeasonInfo(season,year))));

        return await result.Match(
            _ => req.Ok(),
            error => error.ToResponse(req, _logger)
        );

    }
}