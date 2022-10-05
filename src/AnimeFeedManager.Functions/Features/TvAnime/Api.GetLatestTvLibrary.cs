using AnimeFeedManager.Application.TvAnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

public class GetLatestTvLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetLatestTvLibrary> _logger;

    public GetLatestTvLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetLatestTvLibrary>();
    }

    [Function("GetLatestTvLibrary")]
    public async Task<HttpResponseData> RunLatest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/latest")] HttpRequestData req)
    {
        return await _mediator.Send(new GetLatestSeasonCollectionQry())
            .ToResponse(req, _logger);
    }
    
    [Function("GetSeasonTvLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _mediator.Send(new GetSeasonCollectionQry(season, year))
            .ToResponse(req,_logger);
    }
}