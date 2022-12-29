using AnimeFeedManager.Application.TvSubscriptions.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class GetInterestedSeries
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetInterestedSeries> _logger;

    public GetInterestedSeries(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetInterestedSeries>();
    }

    [Function("GetInterestedSeries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "tv/interested/{subscriber}")]
        HttpRequestData req,
        string subscriber
    )
    {
        return await req
            .WithAuthenticationCheck(new GetInterestedSeriesQry(subscriber))
            .BindAsync(r => _mediator.Send(r)).ToResponse(req, _logger);
    }
}