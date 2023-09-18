using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Maintenance.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Backend.Functions.Maintenance;

public class CleanProcessedTitles
{
    private readonly IRemoveProcessedTitles _processedTitles;
    private readonly ILogger<CleanProcessedTitles> _logger;

    public CleanProcessedTitles(
        IRemoveProcessedTitles processedTitles,
       ILoggerFactory loggerFactory)
    {
        _processedTitles = processedTitles;
        _logger = loggerFactory.CreateLogger<CleanProcessedTitles>();
    }

    [Function("CleanProcessedTitles")]
    public async Task Run([TimerTrigger("0 30 1 * * *")] TimerInfo timer)
    {
        var result = await _processedTitles.Remove(DateTimeOffset.Now, default);
        result.Match(
            _ =>
            {
                _logger.LogInformation("Processed titles store has been cleaned");
            },
            e => e.LogDomainError(_logger)
        );
    }
}