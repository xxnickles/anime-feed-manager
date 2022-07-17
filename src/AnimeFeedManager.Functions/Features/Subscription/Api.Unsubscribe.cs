using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "unsubscribe")] HttpRequestData req)
    {
        var dto = await Serializer.FromJson<SubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        var command = new UnsubscribeCmd(dto.UserId, dto.Series);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}