using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Series;

public sealed class GetLibrary(
    TvLibraryGetter tvLibraryGetter,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GetLibrary>();

    [Function("GetSeasonTvLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/{year:int}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await tvLibraryGetter.GetForSeason(season,year)
            .ToResponse(req,_logger);
    }
}