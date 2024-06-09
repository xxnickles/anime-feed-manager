using AnimeFeedManager.Features.Ovas.Library;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Series;

public sealed class GetLibrary(
    OvasLibraryGetter ovasLibraryGetter,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GetLibrary>();

    [Function("GetSeasonOvasLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/{year:int}/{season}")]
        HttpRequestData req,
        string season,
        ushort year,
        CancellationToken token)
    {
        return await ovasLibraryGetter.GetForSeason(season,year, token)
            .ToResponse(req,_logger);
    }
}