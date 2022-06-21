using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions;

public class Unsubscribe
{
    private readonly IMediator _mediator;

    public Unsubscribe(IMediator mediator) => _mediator = mediator;

    [FunctionName("Unsubscribe")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "unsubscribe")] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = JsonConvert.DeserializeObject<Application.Subscriptions.Commands.Unsubscribe>(requestBody);

        return await _mediator.Send(command).ToActionResult(log);
    }
}