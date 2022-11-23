using AnimeFeedManager.Application.MoviesLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Movies;

public class GetSeasonMoviesLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetSeasonMoviesLibrary> _logger;
    
    public GetSeasonMoviesLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetSeasonMoviesLibrary>();
    }
    
    [Function("GetSeasonMoviesLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Movies/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _mediator.Send(new GetMoviesCollectionHandlerQry(season, year))
            .ToResponse(req,_logger);
    }
}