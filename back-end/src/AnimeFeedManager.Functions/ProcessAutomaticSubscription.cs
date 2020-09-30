using System.Threading.Tasks;
using AnimeFeedManager.Application.Subscriptions.Commands;
using AnimeFeedManager.Core.Dto;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions
{
    public class ProcessAutomaticSubscription
    {
        private readonly IMediator _mediator;

        public ProcessAutomaticSubscription(IMediator mediator) => _mediator = mediator;

        [FunctionName("ProcessAutomaticSubscription")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run([QueueTrigger("to-subscribe")] SubscriptionDto newSubscription, ILogger log)
        {
            log.LogInformation($"Automated subscription to {newSubscription.Series} for user {newSubscription.UserId}");
            var command = new MergeSubscription(newSubscription.UserId,newSubscription.Series);
            var result = await _mediator.Send(command);
            result.Match(
                _ => log.LogInformation($"{newSubscription.UserId} has subscribed to {newSubscription.Series} automatically"),
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );
        }
    }
}
