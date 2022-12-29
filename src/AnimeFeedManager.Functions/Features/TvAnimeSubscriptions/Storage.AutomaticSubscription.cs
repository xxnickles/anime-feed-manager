using AnimeFeedManager.Application.TvSubscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class AutomaticSubscription
{
    private readonly IMediator _mediator;
    private readonly ILogger<AutomaticSubscription> _logger;

    public AutomaticSubscription(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<AutomaticSubscription>();
    }

    [Function("AutomaticSubscription")]
    public async Task Run([QueueTrigger(QueueNames.ToSubscribe, Connection = "AzureWebJobsStorage")] TvSubscriptionDto newTvSubscription)
    {
        _logger.LogInformation("Automated subscription to {SubscriptionSeries} for user {UserId}", newTvSubscription.Series, newTvSubscription.UserId);
        var command = new MergeSubscriptionCmd(newTvSubscription.UserId, newTvSubscription.Series);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{UserId} has subscribed to {Series} automatically", newTvSubscription.UserId, newTvSubscription.Series),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}