using AnimeFeedManager.Functions.Extensions;
using MediatR;
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
    private readonly ILogger<RemoveInterested> _logger;

    public RemoveInterested(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<RemoveInterested>();
    }

    [Function("RemoveInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "removeInterested")] HttpRequestData req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var command = JsonConvert.DeserializeObject<Application.Subscriptions.Commands.RemoveInterested>(requestBody);

        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}