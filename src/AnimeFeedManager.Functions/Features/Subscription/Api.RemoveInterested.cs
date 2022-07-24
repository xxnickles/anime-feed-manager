using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "removeInterested")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<SubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        
        return await req
            .WithAuthenticationCheck(new RemoveInterestedCmd(dto.UserId, dto.Series))
            .BindAsync(r => _mediator.Send(r)).ToResponse(req, _logger);
    }
}