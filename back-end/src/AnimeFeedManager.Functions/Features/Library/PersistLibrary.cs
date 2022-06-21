using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions.Features.Library;

public class PersistLibrary
{
    private readonly IMediator _mediator;

    public PersistLibrary(IMediator mediator) => _mediator = mediator;

    [FunctionName("PersistLibrary")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [QueueTrigger(QueueNames.AnimeLibrary)] AnimeInfoStorage animeInfo,
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