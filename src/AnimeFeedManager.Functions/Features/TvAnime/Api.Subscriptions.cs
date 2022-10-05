using AnimeFeedManager.Application.TvSubscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "tv/subscriptions")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<SubscriptionDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        return await req
            .WithAuthenticationCheck(new MergeSubscriptionCmd(dto.UserId, dto.Series))
            .BindAsync(command => _mediator.Send(command))
            .ToResponse(req, _logger);
    }
}