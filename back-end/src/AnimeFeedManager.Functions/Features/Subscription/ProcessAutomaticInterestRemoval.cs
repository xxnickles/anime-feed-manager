using AnimeFeedManager.Core.Dto;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Models;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Subscription;

public class ProcessAutomaticInterestRemoval
{
    private readonly IMediator _mediator;

    public ProcessAutomaticInterestRemoval(IMediator mediator) => _mediator = mediator;

    [FunctionName("ProcessAutomaticInterestRemoval")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run([QueueTrigger(QueueNames.InterestRemove)] SubscriptionDto newSubscription, ILogger log)
    {
        log.LogInformation("Automated removed interest for {Series} for user {UserId}", newSubscription.Series, newSubscription.UserId);
        var command = new Application.Subscriptions.Commands.RemoveInterested(newSubscription.UserId, newSubscription.Series);
        var result = await _mediator.Send(command);
        result.Match(
            _ => log.LogInformation("{Series} has been removed as interest for user {UserId} automatically", newSubscription.Series, newSubscription.UserId),
            e => log.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}