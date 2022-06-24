using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Library;

public class PersistLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersistLibrary> _logger;

    public PersistLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<PersistLibrary>();
    }


    [Function("PersistLibrary")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [QueueTrigger(QueueNames.AnimeLibrary)] AnimeInfoStorage animeInfo
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", animeInfo.Title);
        var command = new MergeAnimeInfo(animeInfo);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{AnimeInfoTitle} has been stored", animeInfo.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}