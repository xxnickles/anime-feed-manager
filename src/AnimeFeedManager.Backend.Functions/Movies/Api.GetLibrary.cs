using AnimeFeedManager.Features.Movies.Library;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Movies;

public sealed class GetLibrary
{
    private readonly MoviesLibraryGetter _moviesLibraryGetter;
    private readonly ILogger _logger;
    
    public GetLibrary(
        MoviesLibraryGetter moviesLibraryGetter, 
        ILoggerFactory loggerFactory )
    {
        _moviesLibraryGetter = moviesLibraryGetter;
        _logger = loggerFactory.CreateLogger<GetLibrary>();
    }
    
    [Function("GetSeasonMoviesLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "movies/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _moviesLibraryGetter.GetForSeason(season,year)
            .ToResponse(req,_logger);
    }
}