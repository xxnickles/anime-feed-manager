using AnimeFeedManager.Application.TvAnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Seasons;

public class GetSeasonLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetSeasonLibrary> _logger;

    public GetSeasonLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetSeasonLibrary>();
    } 

    [Function("GetSeasonLibrary")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "library/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return _mediator.Send(new GetSeasonCollectionQry(season, year))
            .ToResponse(req,_logger);
    }
}