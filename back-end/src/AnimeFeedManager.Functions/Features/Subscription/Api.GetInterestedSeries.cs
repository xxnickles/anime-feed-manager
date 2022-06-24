using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetInterestedSeries
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetInterestedSeries> _logger;

    public GetInterestedSeries(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetInterestedSeries>();
    }

    [Function("GetInterestedSeries")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "interested/{subscriber}")]
        HttpRequestData req,
        string subscriber
        )
    {
        return _mediator.Send(new Application.Subscriptions.Queries.GetInterestedSeries(subscriber))
            .ToResponse(req,_logger);
    }
}