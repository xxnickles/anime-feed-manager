using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetSubscribedSeries
{
    private readonly IMediator _mediator;

    public GetSubscribedSeries(IMediator mediator) => _mediator = mediator;

    [FunctionName("GetSubscribedSeries")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber,
        ILogger log)
    {
        return _mediator.Send(new Application.Subscriptions.Queries.GetSubscribedSeries(subscriber))
            .ToResponse(req,log);
    }
}