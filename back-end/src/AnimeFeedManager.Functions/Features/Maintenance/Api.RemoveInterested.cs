using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class RemoveInterested
{
    private readonly IMediator _mediator;

    public RemoveInterested(IMediator mediator) => _mediator = mediator;

    [FunctionName("RemoveInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "removeInterested")] HttpRequestData req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = JsonConvert.DeserializeObject<Application.Subscriptions.Commands.RemoveInterested>(requestBody);

        return await _mediator.Send(command).ToResponse(req,log);
    }
}