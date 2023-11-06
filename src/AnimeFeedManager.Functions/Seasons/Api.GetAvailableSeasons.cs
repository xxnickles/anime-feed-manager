using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Seasons;

public sealed class GetAvailableSeasons(SeasonsGetter seasonsGetter, ILoggerFactory loggerFactory)
{
    private readonly ILogger<GetAvailableSeasons> _logger = loggerFactory.CreateLogger<GetAvailableSeasons>();

    [Function("GetAvailableSeasons")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "seasons")]
        HttpRequestData req)
    {
        return seasonsGetter.GetAvailable()
            .ToResponse(req, _logger);
    }

}