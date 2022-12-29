using AnimeFeedManager.Application.TvSubscriptions.Commands;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnimeSubscriptions;

public class DeleteInterested
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteInterested> _logger;

    public DeleteInterested(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<DeleteInterested>();
    }

    [Function("DeleteInterested")]
    public async Task Run([QueueTrigger(QueueNames.DeleteInterested, Connection = "AzureWebJobsStorage")] TvSubscriptionDto newTvSubscription)
    {
        _logger.LogInformation("Automated interest remove for {Series} for user {UserId}", newTvSubscription.Series, newTvSubscription.UserId);
        var command = new DeleteInterestedCmd(newTvSubscription.UserId, newTvSubscription.Series);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{Series} has been removed from interest list for user {UserId} automatically", newTvSubscription.Series, newTvSubscription.UserId),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}