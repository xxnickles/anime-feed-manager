using AnimeFeedManager.Application.OvasLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class GetSeasonOvasLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetSeasonOvasLibrary> _logger;
    
    public GetSeasonOvasLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetSeasonOvasLibrary>();
    }
    
    [Function("GetSeasonOvasLibrary")]
    public async Task<HttpResponseData> RunSeason(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return await _mediator.Send(new GetOvasCollectionHandlerQry(season, year))
            .ToResponse(req,_logger);
    }
}