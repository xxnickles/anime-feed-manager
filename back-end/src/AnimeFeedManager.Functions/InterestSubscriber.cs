using AnimeFeedManager.Application.Subscriptions.Queries;
using AnimeFeedManager.Core.Dto;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class InterestSubscriber
    {
        private readonly IMediator _mediator;

        public InterestSubscriber(IMediator mediator) => _mediator = mediator;

        [FunctionName("InterestSubscriber")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run(
            [QueueTrigger(QueueNames.ProcessAutoSubscriber)] string processResult,
            [Queue(QueueNames.ToSubscribe)] IAsyncCollector<SubscriptionDto> toSubscribeQueueCollector,
            [Queue(QueueNames.InterestRemove)] IAsyncCollector<SubscriptionDto> interestToRemoveSeasonCollector,
            ILogger log)
        {
            if (processResult == ProcessResult.Ok)
            {
                log.LogInformation("Titles were updated and checking of status process was completed, checking if there are interested series that match new titles");
                var interestedSeries = await _mediator.Send(new GetAllInterestedSeries());

                interestedSeries.Match(async series =>
                    {
                        var result = await _mediator.Send(new SubscribeToInterest(series));
                        result.Match(
                            r =>
                            {
                                if (r.Any())
                                {
                                    var toSubscribe = r.Select(s => new SubscriptionDto(s.UserId, s.FeedTitle))
                                        .ToImmutableList();
                                    QueueStorage.StoreInQueue(toSubscribe, toSubscribeQueueCollector, log, x => $"Queueing subscription to {x.Series} for {x.UserId} ");

                                    var toRemoveFromInterest = r.Select(s => new SubscriptionDto(s.UserId, s.InterestedTitle))
                                        .ToImmutableList();
                                    QueueStorage.StoreInQueue(toRemoveFromInterest, interestToRemoveSeasonCollector, log, x => $"Queueing {x.Series} for remove for interest for {x.UserId} ");
                                }
                                else
                                {
                                    log.LogInformation($"Nothing to process");
                                }

                            },
                            e => log.LogError($"[{e.CorrelationId}]: {e.Message}"));
                    },
                    e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
                );
            }
            else
            {
                log.LogInformation("Title process failed, nothing to update");
            }
        }
    }
}
