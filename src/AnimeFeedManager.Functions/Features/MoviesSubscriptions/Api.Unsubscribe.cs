using AnimeFeedManager.Application.MoviesSubscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.MoviesSubscriptions;

public class Unsubscribe
{
    private readonly IMediator _mediator;
    private readonly ILogger<Unsubscribe> _logger;

    public Unsubscribe(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<Unsubscribe>();
    }

    [Function("MoviesUnsubscribe")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "Movies/subscriptions/unsubscribe")]
        HttpRequestData req)
    {
        var dto = await Serializer.FromJson<ShortSeriesUnsubscribeDto>(req.Body);
        ArgumentNullException.ThrowIfNull(dto);
        return await req
            .WithAuthenticationCheck(new UnsubscribeCmd(dto.UserId, dto.Series))
            .BindAsync(command => _mediator.Send(command))
            .ToResponse(req, _logger);
    }
}