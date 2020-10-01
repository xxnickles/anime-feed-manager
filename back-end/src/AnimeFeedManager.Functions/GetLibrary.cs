using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
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
            [TimerTrigger("1 * * * * *")] TimerInfo timer,
            [Queue("anime-library")] IAsyncCollector<AnimeInfoStorage> animeQueueCollector,
            [Queue("available-seasons")] IAsyncCollector<SeasonInfo> availableSeasonCollector,
            ILogger log)
        {
            var result = await _mediator.Send(new GetExternalLibrary());
            result.Match(
                v =>
                {
                    var seasonInfo = ExtractSeasonInformation(v.First());
                    availableSeasonCollector.AddAsync(seasonInfo);
                    QueueStorage.StoreInQueue(v, animeQueueCollector, log, x => $"Queueing {x.Title}");
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
