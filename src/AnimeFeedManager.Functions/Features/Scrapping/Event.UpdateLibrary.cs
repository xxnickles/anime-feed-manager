using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class UpdateLibrary
{
    private readonly ILogger<UpdateLibrary> _logger;

    public UpdateLibrary(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<UpdateLibrary>();
    }

    [Function("UpdateLibraryTimer")]
    [QueueOutput(QueueNames.LibraryUpdate, Connection = "AzureWebJobsStorage")]
    public LibraryUpdate Run(
        [TimerTrigger("0 0 2 * * SAT")] TimerInfo timer
    )
    {
        _logger.LogInformation("Automated Update of Library (Timer trigger {timer})",
            timer.ScheduleStatus?.LastUpdated ?? DateTime.Now);
        return new LibraryUpdate(LibraryUpdateType.Full);
    }
}