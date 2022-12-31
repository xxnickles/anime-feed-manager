using AnimeFeedManager.Application.MoviesSubscriptions.Queries;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.MoviesSubscriptions;

public class GetSubscriptions
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetSubscriptions> _logger;

    public GetSubscriptions(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetSubscriptions>();
    }

    [Function("GetMoviesSubscriptions")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Movies/subscriptions/{subscriber}")]
        HttpRequestData req,
        string subscriber)
    {
        return await req
            .WithAuthenticationCheck(new GetSubscriptionsQry(subscriber))
            .BindAsync(request => _mediator.Send(request))
            .ToResponse(req, _logger);
    }
}