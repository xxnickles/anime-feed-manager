using System;
using System.Text.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class Subscriptions
{
    private readonly IMediator _mediator;
    private readonly ILogger<Subscriptions> _logger;

    public Subscriptions(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<Subscriptions>();
    }

    [Function("MergeSubscription")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "subscriptions")]
        HttpRequestData req)
    {
        var command = await JsonSerializer.DeserializeAsync<MergeSubscription>(req.Body);
        ArgumentNullException.ThrowIfNull(command);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}