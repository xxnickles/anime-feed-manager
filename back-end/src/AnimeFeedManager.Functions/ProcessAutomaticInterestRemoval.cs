using AnimeFeedManager.Core.Dto;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Models;

namespace AnimeFeedManager.Functions
{
    public class ProcessAutomaticInterestRemoval
    {
        private readonly IMediator _mediator;

        public ProcessAutomaticInterestRemoval(IMediator mediator) => _mediator = mediator;

        [FunctionName("ProcessAutomaticInterestRemoval")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run([QueueTrigger(QueueNames.InterestRemove)] SubscriptionDto newSubscription, ILogger log)
        {
            log.LogInformation($"Automated removed interest for {newSubscription.Series} for user {newSubscription.UserId}");
            var command = new Application.Subscriptions.Commands.RemoveInterested(newSubscription.UserId,newSubscription.Series);
            var result = await _mediator.Send(command);
            result.Match(
                _ => log.LogInformation($"{newSubscription.Series} has been removed as interest for user {newSubscription.UserId} automatically"),
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );
        }
    }
}
