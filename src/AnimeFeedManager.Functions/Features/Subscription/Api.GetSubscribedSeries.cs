using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class GetSubscribedSeries
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetInterestedSeries> _logger;

    public GetSubscribedSeries(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetInterestedSeries>();
    }

    [Function("GetSubscribedSeries")]
    public Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return req
            .WithAuthenticationCheck(new Application.Subscriptions.Queries.GetSubscribedSeriesQry(subscriber))
            .BindAsync(r => _mediator.Send(r)).ToResponse(req, _logger);
    }
}