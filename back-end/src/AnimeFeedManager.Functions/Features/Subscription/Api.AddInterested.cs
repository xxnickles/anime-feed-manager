using System.IO;
using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class AddInterested
{
    private readonly IMediator _mediator;

    public AddInterested(IMediator mediator) => _mediator = mediator;

    [FunctionName("MergeInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "interested")]
        HttpRequestData req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = JsonConvert.DeserializeObject<MergeInterestedSeries>(requestBody);

        return await _mediator.Send(command).ToResponse(req,log);
    }
}