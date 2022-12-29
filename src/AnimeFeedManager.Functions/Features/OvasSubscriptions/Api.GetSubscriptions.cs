using AnimeFeedManager.Application.OvasSubscriptions.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.OvasSubscriptions;

public class GetSubscriptions
{
    private readonly IMediator _mediator;
    private readonly ILogger<Subscribe> _logger;

    public GetSubscriptions(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<Subscribe>();
    }

    [Function("GetOvasSubscriptions")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ovas/subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await req
            .WithAuthenticationCheck(new GetSubscriptionsQry(subscriber))
            .BindAsync(request => _mediator.Send(request))
            .ToResponse(req, _logger);
    }
}