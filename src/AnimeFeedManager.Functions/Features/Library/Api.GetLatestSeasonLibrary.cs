using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class GetLatestSeasonLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetLatestSeasonLibrary> _logger;

    public GetLatestSeasonLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetLatestSeasonLibrary>();
    }

    [Function("GetLatestSeasonLibrary")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "library/latest")] HttpRequestData req)
    {
        return _mediator.Send(new GetLatestSeasonCollectionQry())
            .ToResponse(req, _logger);
    }
}