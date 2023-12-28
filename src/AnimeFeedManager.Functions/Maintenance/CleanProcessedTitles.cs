using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Maintenance.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Maintenance;

public class CleanProcessedTitles(
    IRemoveProcessedTitles processedTitles,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<CleanProcessedTitles> _logger = loggerFactory.CreateLogger<CleanProcessedTitles>();

    [Function("CleanProcessedTitles")]
    public async Task Run([TimerTrigger("0 30 1 * * *")] TimerInfo timer)
    {
        var result = await processedTitles.Remove(DateTimeOffset.Now, default);
        result.Match(
            _ =>
            {
                _logger.LogInformation("Processed titles store has been cleaned");
            },
            e => e.LogError(_logger)
        );
    }
}