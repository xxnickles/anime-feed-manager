using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Titles;

public sealed class UpdateTitles(ScrapSeasonTitles titlesScrapper, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<UpdateTitles>();

    [Function("UpdateTitles")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "tv/titles")]
        HttpRequestData req, CancellationToken token)
    {
        _logger.LogInformation("Automated Update of Titles (Manual trigger)");

        var result = await req.AllowAdminOnly().BindAsync(_ => titlesScrapper.Scrap(token));
        return await result.ToResponse(req, _logger);
    }
}