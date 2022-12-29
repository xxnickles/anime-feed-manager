using AnimeFeedManager.Application.TvSubscriptions.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

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
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await req
            .WithAuthenticationCheck(new GetSubscribedSeriesQry(subscriber))
            .BindAsync(request => _mediator.Send(request))
            .ToResponse(req, _logger);
    }
}