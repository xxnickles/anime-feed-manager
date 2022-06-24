using System.IO;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class Unsubscribe
{
    private readonly IMediator _mediator;

    public Unsubscribe(IMediator mediator) => _mediator = mediator;

    [FunctionName("Unsubscribe")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "unsubscribe")] HttpRequestData req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = JsonConvert.DeserializeObject<Application.Subscriptions.Commands.Unsubscribe>(requestBody);

        return await _mediator.Send(command).ToResponse(req,log);
    }
}