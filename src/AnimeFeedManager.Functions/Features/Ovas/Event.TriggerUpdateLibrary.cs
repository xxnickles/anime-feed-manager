using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Ovas;

public class TriggerUpdateOvasLibrary
{
    private readonly ILogger<TriggerUpdateOvasLibrary> _logger;

    public TriggerUpdateOvasLibrary(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TriggerUpdateOvasLibrary>();
    }

    [Function("TriggerUpdateOvasLibrary")]
    [QueueOutput(QueueNames.OvasLibraryUpdate, Connection = "AzureWebJobsStorage")]
    public OvasUpdate Run(
        [TimerTrigger("0 0 2 * * SAT")] TimerInfo timer
    )
    {
        _logger.LogInformation("Automated Update of Library (Timer trigger {timer})",
            timer.ScheduleStatus?.LastUpdated ?? DateTime.Now);
        return new OvasUpdate();
    }
}