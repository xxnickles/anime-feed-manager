using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetSubscribedSeries
{
    private readonly IMediator _mediator;

    public GetSubscribedSeries(IMediator mediator) => _mediator = mediator;

    [FunctionName("GetSubscribedSeries")]
    public Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "subscriptions/{subscriber}")]
        HttpRequest req,
        string subscriber,
        ILogger log)
    {
        return _mediator.Send(new Application.Subscriptions.Queries.GetSubscribedSeries(subscriber))
            .ToActionResult(log);
    }
}