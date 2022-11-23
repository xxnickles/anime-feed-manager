using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Movies;

public class TriggerUpdateMoviesLibrary
{
    private readonly ILogger<TriggerUpdateMoviesLibrary> _logger;

    public TriggerUpdateMoviesLibrary(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<TriggerUpdateMoviesLibrary>();
    }

    [Function("TriggerUpdateMoviesLibrary")]
    [QueueOutput(QueueNames.MoviesLibraryUpdate, Connection = "AzureWebJobsStorage")]
    public MoviesUpdate Run(
        [TimerTrigger("0 0 2 * * SAT")] TimerInfo timer
    )
    {
        _logger.LogInformation("Automated Update of Library (Timer trigger {timer})",
            timer.ScheduleStatus?.LastUpdated ?? DateTime.Now);
        return new MoviesUpdate(ShortSeriesUpdateType.Latest, new NullSeasonInfo());
    }
}