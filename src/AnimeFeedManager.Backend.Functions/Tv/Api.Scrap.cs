using AnimeFeedManager.Backend.Functions.ResponseExtensions;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Tv;

public class Scrap
{
    private readonly TvLibraryUpdater _libraryUpdater;
    private readonly ILogger _logger;
    
    public Scrap(
        TvLibraryUpdater libraryUpdater,
        ILoggerFactory loggerFactory)
    {
        _libraryUpdater = libraryUpdater;
        _logger = loggerFactory.CreateLogger<Scrap>();
    }

    [Function("ScrapLatestTvSeason")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Library (Manual trigger)");

        var result = await _libraryUpdater.Update(new Latest());
        
        return await result.Match(
            _ => req.Ok(),
            error => error.ToResponse(req, _logger)
        );
    }
    
    [Function("ScrapCustomTvSeason")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/library/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update Library (Manual trigger) for Custom Season");
        var result = await req.AllowAdminOnly()
            .BindAsync(_ => _libraryUpdater.Update(new BySeason(season,year)));

        return await result.Match(
            _ => req.Ok(),
            error => error.ToResponse(req, _logger)
        );

    }
}