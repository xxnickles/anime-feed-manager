using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class AddInterested
{
    private readonly IMediator _mediator;
    private readonly ILogger<AddInterested> _logger;

    public AddInterested(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<AddInterested>();
    }

    [Function("MergeInterested")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "interested")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<SubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        var command = new MergeInterestedSeriesCmd(dto.UserId, dto.Series);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}