using System.Threading.Tasks;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetSubscribedSeries
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetInterestedSeries> _logger;

    public GetSubscribedSeries(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetInterestedSeries>();
    }

    [Function("GetSubscribedSeries")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return _mediator.Send(new Application.Subscriptions.Queries.GetSubscribedSeriesQry(subscriber))
            .ToResponse(req,_logger);
    }
}