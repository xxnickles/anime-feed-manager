using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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
    public async Task Run(
        [QueueTrigger(QueueNames.AnimeLibrary, Connection = "AzureWebJobsStorage")] AnimeInfoStorage animeInfo
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", animeInfo.Title);
        var command = new MergeAnimeInfoCmd(animeInfo);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{AnimeInfoTitle} has been stored", animeInfo.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}