using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions
{
    public class PersistLibrary
    {
        private readonly IMediator _mediator;

        public PersistLibrary(IMediator mediator) => _mediator = mediator;

        [FunctionName("PersistLibrary")]
        [StorageAccount("AzureWebJobsStorage")]
        public async Task Run(
            [QueueTrigger("anime-library")] AnimeInfoStorage animeInfo,
            ILogger log)
        {
            log.LogInformation($"storing {animeInfo.Title}");
            var command = new MergeAnimeInfo(animeInfo);
            var result = await _mediator.Send(command);
            result.Match(
                _ => log.LogInformation($"{animeInfo.Title} has been stored"),
                e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
            );
        }
    }
}