using System;
using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Functions.Extensions;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
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
        [HttpTrigger(AuthorizationLevel.Function, "post", "put", Route = "interested")]
        HttpRequestData req)
    {
        var command = await Serializer.FromJson<MergeInterestedSeriesCmd>(req.Body);
        ArgumentNullException.ThrowIfNull(command);
        return await _mediator.Send(command).ToResponse(req, _logger);
    }
}