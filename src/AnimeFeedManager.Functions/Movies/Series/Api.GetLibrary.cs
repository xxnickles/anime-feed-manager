using AnimeFeedManager.Features.Movies.Library;
using AnimeFeedManager.Functions.ResponseExtensions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Movies.Series;

public sealed class GetLibrary(
    MoviesLibraryGetter moviesLibraryGetter,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GetLibrary>();

    [Function("GetSeasonMoviesLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "movies/{year:int}/{season}")]
        HttpRequestData req,
        string season,
        ushort year,
        CancellationToken token)
    {
        return await moviesLibraryGetter.GetForSeason(season,year, token)
            .ToResponse(req,_logger);
    }
}