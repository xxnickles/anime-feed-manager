using System.Threading.Tasks;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

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
        return _mediator.Send(new Application.Subscriptions.Queries.GetInterestedSeriesQry(subscriber))
            .ToResponse(req,_logger);
    }
}