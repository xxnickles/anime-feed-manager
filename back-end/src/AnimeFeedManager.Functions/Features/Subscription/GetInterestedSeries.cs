using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetInterestedSeries
{
    private readonly IMediator _mediator;

    public GetInterestedSeries(IMediator mediator) => _mediator = mediator;

    [FunctionName("GetInterestedSeries")]
    public Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "interested/{subscriber}")]
        HttpRequest req,
        string subscriber,
        ILogger log)
    {
        return _mediator.Send(new Application.Subscriptions.Queries.GetInterestedSeries(subscriber))
            .ToActionResult(log);
    }
}