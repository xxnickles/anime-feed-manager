using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions;

public class UpdateAnimeStatus
{
    private readonly IMediator _mediator;

    public UpdateAnimeStatus(IMediator mediator) => _mediator = mediator;

    [FunctionName("UpdateAnimeStatus")]
    [StorageAccount("AzureWebJobsStorage")]
    [return: Queue(QueueNames.ProcessAutoSubscriber, Connection = "AzureWebJobsStorage")]
    public async Task<string> Run(
        [QueueTrigger(QueueNames.TitleProcess)] string processResult,
        [Queue(QueueNames.AnimeLibrary)] IAsyncCollector<AnimeInfoStorage> animeQueueCollector,

        ILogger log)
    {
        if (processResult == ProcessResult.Ok)
        {
            log.LogInformation("Titles source has been updated. Verifying whether series need to be marked as completed");

            var result = await _mediator.Send(new UpdateStatus());
            return result.Match(
                v =>
                {
                    QueueStorage.StoreInQueue(v, animeQueueCollector, log, x => $"Queueing for update {x.Title}");
                    return ProcessResult.Ok;
                },
                e =>
                {
                    log.LogError($"[{e.CorrelationId}]: {e.Message}");
                    return ProcessResult.Failure;
                });
        }

        log.LogInformation("Title process failed, series status is not going to be updated");
        return ProcessResult.NoChanges;
    }

}