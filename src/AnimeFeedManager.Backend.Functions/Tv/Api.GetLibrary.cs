using AnimeFeedManager.Features.Tv.Library;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Tv;

public class GetLibrary
{
    private readonly TvLibraryGetter _tvLibraryGetter;
    private readonly ILogger _logger;
    
    public GetLibrary(TvLibraryGetter tvLibraryGetter,  ILoggerFactory loggerFactory )
    {
        _tvLibraryGetter = tvLibraryGetter;
        _logger = loggerFactory.CreateLogger<GetLibrary>();
    }
    
    [Function("GetSeasonTvLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _tvLibraryGetter.GetForSeason(season,year)
            .ToResponse(req,_logger);
    }
}