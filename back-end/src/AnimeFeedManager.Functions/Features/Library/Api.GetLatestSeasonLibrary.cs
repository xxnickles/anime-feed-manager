using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
        return _mediator.Send(new GetLatestSeasonCollection())
            .ToResponse(req, _logger);
    }
}