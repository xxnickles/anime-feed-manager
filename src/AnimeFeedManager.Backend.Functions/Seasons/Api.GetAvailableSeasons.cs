using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Seasons;

public sealed class GetAvailableSeasons
{
    private readonly SeasonsGetter _seasonsGetter;
    private readonly ILogger<GetAvailableSeasons> _logger;

    public GetAvailableSeasons(SeasonsGetter seasonsGetter, ILoggerFactory loggerFactory)
    {
        _seasonsGetter = seasonsGetter;
        _logger = loggerFactory.CreateLogger<GetAvailableSeasons>();
    }

    [Function("GetAvailableSeasons")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "seasons")]
        HttpRequestData req)
    {
        return _seasonsGetter.GetAvailable()
            .ToResponse(req, _logger);
    }

}