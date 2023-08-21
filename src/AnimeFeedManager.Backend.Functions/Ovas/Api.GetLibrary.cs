using AnimeFeedManager.Features.Ovas.Library;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Ovas;

public class GetLibrary
{
    private readonly OvasLibraryGetter _ovasLibraryGetter;
    private readonly ILogger _logger;
    
    public GetLibrary(
        OvasLibraryGetter ovasLibraryGetter, 
        ILoggerFactory loggerFactory )
    {
        _ovasLibraryGetter = ovasLibraryGetter;
        _logger = loggerFactory.CreateLogger<GetLibrary>();
    }
    
    [Function("GetSeasonOvasLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _ovasLibraryGetter.GetForSeason(season,year)
            .ToResponse(req,_logger);
    }
}