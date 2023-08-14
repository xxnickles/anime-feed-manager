using AnimeFeedManager.Features.Ovas.Scrapping;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Ovas;

public class Scrap
{
    private readonly OvasLibraryUpdater _libraryUpdater;
    private readonly ILogger _logger;
    
    public Scrap(
        OvasLibraryUpdater libraryUpdater,
        ILoggerFactory loggerFactory)
    {
        _libraryUpdater = libraryUpdater;
        _logger = loggerFactory.CreateLogger<Scrap>();
    }

    [Function("ScrapLatestOvasSeason")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ovas/library")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Ovas Library (Manual trigger)");

        var result = await _libraryUpdater.Update(new Latest());
        
        return await result.Match(
            _ => req.Ok(),
            error => error.ToResponse(req, _logger)
        );
    }
    
    [Function("ScrapCustomOvasSeason")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ovas/library/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update Ovas Library (Manual trigger) for Custom Season");
        var result = await req.AllowAdminOnly()
            .BindAsync(_ => _libraryUpdater.Update(new BySeason(season,year)));

        return await result.Match(
            _ => req.Ok(),
            error => error.ToResponse(req, _logger)
        );

    }
}