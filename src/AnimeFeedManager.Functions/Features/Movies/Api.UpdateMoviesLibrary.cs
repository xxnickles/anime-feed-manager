using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Movies;

public class UpdateMoviesLibraryOutput
{
    [QueueOutput(QueueNames.MoviesLibraryUpdate)]
    public MoviesUpdate? UpdatePayload { get; set; }

    public HttpResponseData? HttpResponse { get; set; }
}

public class UpdateMoviesLibrary
{
    private readonly ILogger _logger;

    public UpdateMoviesLibrary(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<UpdateMoviesLibrary>();
    }


    [Function("UpdateSeasonMoviesLibrary")]
    public async Task<UpdateMoviesLibraryOutput> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Movies/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        _logger.LogInformation("Automated Update of latest Movie library (Manual trigger)");

        var result = await req.AllowAdminOnly();

        return await result.Match(
            _ => OkResponse(req, new MoviesUpdate(ShortSeriesUpdateType.Season, new SeasonInfoDto(season, year))),
            e => ErrorResponse(req, e)
        );
    }

    private static async Task<UpdateMoviesLibraryOutput> OkResponse(HttpRequestData req, MoviesUpdate updatePayload)
    {
        return new UpdateMoviesLibraryOutput
        {
            UpdatePayload = updatePayload,
            HttpResponse = await req.Ok()
        };
    }

    private async Task<UpdateMoviesLibraryOutput> ErrorResponse(HttpRequestData req, DomainError error)
    {
        return new UpdateMoviesLibraryOutput
        {
            UpdatePayload = null,
            HttpResponse = await error.ToResponse(req, _logger)
        };
    }
}