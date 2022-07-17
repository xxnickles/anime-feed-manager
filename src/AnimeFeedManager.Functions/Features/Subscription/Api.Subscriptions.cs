using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "subscriptions")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<SubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        var command = new MergeSubscriptionCmd(dto.UserId, dto.Series);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}