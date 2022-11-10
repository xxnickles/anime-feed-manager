using AnimeFeedManager.Application.OvasLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class PersistOvas
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersistOvas> _logger;

    public PersistOvas(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<PersistOvas>();
    }


    [Function("PersistOvas")]
    public async Task Run(
        [QueueTrigger(QueueNames.OvasLibraryUpdates, Connection = "AzureWebJobsStorage")] OvaStorage ovaStorage
        )
    {
        _logger.LogInformation("storing {AnimeInfoTitle}", ovaStorage.Title);
        var command = new MergeOvaCmd(ovaStorage);
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("Ova '{OvaTitle}' has been stored", ovaStorage.Title),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}