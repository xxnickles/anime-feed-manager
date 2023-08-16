using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Tv.Titles;

public class UpdateTitles
{
    private readonly ScrapSeasonTitles _titlesScrapper;
    private readonly ILogger _logger;
    
    public UpdateTitles(ScrapSeasonTitles titlesScrapper,  ILoggerFactory loggerFactory )
    {
        _titlesScrapper = titlesScrapper;
        _logger = loggerFactory.CreateLogger<UpdateTitles>();
    }
    
    [Function("UpdateTitles")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/titles")]
        HttpRequestData req)
    {
        _logger.LogInformation("Automated Update of Titles (Manual trigger)");

        var result = await req.AllowAdminOnly().BindAsync(_ => _titlesScrapper.Scrap());
        return await result.ToResponse(req, _logger);
    }
}