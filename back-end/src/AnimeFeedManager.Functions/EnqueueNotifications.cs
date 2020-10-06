using AnimeFeedManager.Application.Notifications;
using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using LanguageExt;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class EnqueueNotifications
    {
        private readonly IMediator _mediator;
        public EnqueueNotifications(IMediator mediator) => _mediator = mediator;

        [FunctionName("EnqueueNotifications")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run(
            [TimerTrigger("0 0 18 * * *")] TimerInfo timer,
            [Queue(QueueNames.Notifications)] IAsyncCollector<Notification> queueCollector,
            ILogger log)
        {
            var notifications = await _mediator.Send(new GetNotifications());

            notifications.Match(
                v => QueueStorage.StoreInQueue(v, queueCollector, log,
                    x => $"Queueing subscriptions for {x.Subscriber}"),
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );
        }

    }
}
