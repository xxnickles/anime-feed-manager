using AnimeFeedManager.Old.Features.Maintenance.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Maintenance;

public class CleanProcessedTitles(
    IRemoveProcessedTitles processedTitles,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<CleanProcessedTitles> _logger = loggerFactory.CreateLogger<CleanProcessedTitles>();

    [Function(nameof(CleanProcessedTitles))]
    public async Task Run([TimerTrigger("0 30 1 * * *")] TimerInfo timer, CancellationToken token)
    {
        var result = await processedTitles.Remove(DateTimeOffset.Now, token);
        result.Match(
            _ =>
            {
                _logger.LogInformation("Processed titles store has been cleaned");
            },
            e => e.LogError(_logger)
        );
    }
}