using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.TvAnime;

public class TriggerUpdateLibrary
{
    private readonly ILogger<TriggerUpdateLibrary> _logger;

    public TriggerUpdateLibrary(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TriggerUpdateLibrary>();
    }

    [Function("TriggerUpdateLibrary")]
    [QueueOutput(QueueNames.TvAnimeLibraryUpdate, Connection = "AzureWebJobsStorage")]
    public LibraryUpdate Run(
        [TimerTrigger("0 0 2 * * SAT")] TimerInfo timer
    )
    {
        _logger.LogInformation("Automated Update of Library (Timer trigger {timer})",
            timer.ScheduleStatus?.LastUpdated ?? DateTime.Now);
        return new LibraryUpdate(LibraryUpdateType.Full);
    }
}