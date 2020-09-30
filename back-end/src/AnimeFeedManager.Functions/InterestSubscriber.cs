using System.Collections.Immutable;
using AnimeFeedManager.Application.Subscriptions.Queries;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Dto;

namespace AnimeFeedManager.Functions
{
    public class InterestSubscriber
    {
        private readonly IMediator _mediator;

        public InterestSubscriber(IMediator mediator) => _mediator = mediator;

        [FunctionName("InterestSubscriber")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run(
            [TimerTrigger("0 0 0 * * *")] TimerInfo timer,
            [Queue("to-subscribe")] IAsyncCollector<SubscriptionDto> toSubscribeQueueCollector,
            [Queue("interest-remove")] IAsyncCollector<SubscriptionDto> interestToRemoveSeasonCollector,
            ILogger log)
        {
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

        private SeasonInfo ExtractSeasonInformation(AnimeInfoStorage sample) => new SeasonInfo
        {
            Season = sample.Season,
            Year = sample.Year
        };
    }
}
