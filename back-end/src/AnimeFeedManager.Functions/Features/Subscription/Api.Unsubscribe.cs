using System.Text.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
namespace AnimeFeedManager.Functions.Features.Subscription;

public class Unsubscribe
{
    private readonly IMediator _mediator;
    private readonly ILogger<Unsubscribe> _logger;

    public Unsubscribe(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<Unsubscribe>();
    }

    [Function("Unsubscribe")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "unsubscribe")] HttpRequestData req)
    {
        var command = await JsonSerializer.DeserializeAsync<Application.Subscriptions.Commands.Unsubscribe>(req.Body);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}