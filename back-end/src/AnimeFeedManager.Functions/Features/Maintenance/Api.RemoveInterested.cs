using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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
        var command = await JsonSerializer.DeserializeAsync<Application.Subscriptions.Commands.RemoveInterested>(req.Body);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}