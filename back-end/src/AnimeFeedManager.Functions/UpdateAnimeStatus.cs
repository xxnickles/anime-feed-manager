using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class UpdateAnimeStatus
    {
        private readonly IMediator _mediator;

        public UpdateAnimeStatus(IMediator mediator) => _mediator = mediator;

        [FunctionName("UpdateAnimeStatus")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run(
            [TimerTrigger("0 0 2 * * SUN")] TimerInfo timer,
            [Queue(QueueNames.AnimeLibrary)] IAsyncCollector<AnimeInfoStorage> animeQueueCollector,
           
            ILogger log)
        {
            var result = await _mediator.Send(new UpdateStatus());
            result.Match(
                v =>
                {
                    QueueStorage.StoreInQueue(v, animeQueueCollector, log, x => $"Queueing for update {x.Title}");
                },
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );
        }

    }
}
