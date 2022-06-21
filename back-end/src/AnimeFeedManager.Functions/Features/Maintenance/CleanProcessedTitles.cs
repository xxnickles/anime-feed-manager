using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions.Features.Maintenance;

public class CleanProcessedTitles
{
    private readonly IMediator _mediator;

    public CleanProcessedTitles(IMediator mediator) => _mediator = mediator;

    [FunctionName("CleanProcessedTitles")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [TimerTrigger("0 30 1 * * *")] TimerInfo timer,
        ILogger log)
    {
        var result = await _mediator.Send(new Application.Feed.Commands.CleanProcessedTitles());
        result.Match(
            _ =>
            {
                log.LogInformation("Processed titles store has been cleaned");
            },
            e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
        );
    }
}