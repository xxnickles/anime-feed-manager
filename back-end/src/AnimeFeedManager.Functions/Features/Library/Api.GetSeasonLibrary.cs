using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

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
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "library/{year}/{season}")]
        HttpRequestData req,
        string season,
        ushort year)
    {
        return _mediator.Send(new GetSeasonCollection(season, year))
            .ToResponse(req,_logger);
    }
}