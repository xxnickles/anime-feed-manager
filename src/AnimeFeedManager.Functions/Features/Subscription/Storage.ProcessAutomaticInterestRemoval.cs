using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Core.Dto;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class ProcessAutomaticInterestRemoval
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessAutomaticInterestRemoval> _logger;

    public ProcessAutomaticInterestRemoval(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ProcessAutomaticInterestRemoval>();
    }

    [Function("ProcessAutomaticInterestRemoval")]
    public async Task Run([QueueTrigger(QueueNames.InterestRemove, Connection = "AzureWebJobsStorage")] SubscriptionDto newSubscription)
    {
        _logger.LogInformation("Automated removed interest for {Series} for user {UserId}", newSubscription.Series, newSubscription.UserId);
        var command = new RemoveInterestedCmd(newSubscription.UserId, newSubscription.Series);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{Series} has been removed as interest for user {UserId} automatically", newSubscription.Series, newSubscription.UserId),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}