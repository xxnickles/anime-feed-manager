using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public class GetLibrary
    {
        private readonly IMediator _mediator;

        public GetLibrary(IMediator mediator) => _mediator = mediator;

        [FunctionName("GetLibrary")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run(
            [TimerTrigger("0 0 2 * * SAT")] TimerInfo myTimer,
            [Queue("anime-library")] IAsyncCollector<AnimeInfoStorage> queueCollector,
            ILogger log)
        {
            var result = await _mediator.Send(new GetExternalLibrary());
            result.Match(
                v => QueueStorage.StoreInQueue(v, queueCollector, log, x => $"Queueing {x.Title}"),
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );

        }
    }
}
