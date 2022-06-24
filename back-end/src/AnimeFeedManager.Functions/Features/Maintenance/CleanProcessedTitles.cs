using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class CleanProcessedTitles
{
    private readonly IMediator _mediator;
    private readonly ILogger<CleanProcessedTitles> _logger;

    public CleanProcessedTitles(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<CleanProcessedTitles>();
    }

    [Function("CleanProcessedTitles")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [TimerTrigger("0 30 1 * * *")] TimerInfo timer
        )
    {
        var result = await _mediator.Send(new Application.Feed.Commands.CleanProcessedTitles());
        result.Match(
            _ =>
            {
                _logger.LogInformation("Processed titles store has been cleaned");
            },
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}