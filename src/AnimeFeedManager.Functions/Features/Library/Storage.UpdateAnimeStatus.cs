using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;
public class UpdateAnimeStatusMessages
{
    [QueueOutput(QueueNames.AnimeLibrary)]
    public IEnumerable<string>? AnimeMessages { get; set; }
    [QueueOutput(QueueNames.ProcessAutoSubscriber, Connection = "AzureWebJobsStorage")]
    public string? AutoSubscribeMessages { get; set; }
}

public class UpdateAnimeStatus
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateAnimeStatus> _logger;

    public UpdateAnimeStatus(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<UpdateAnimeStatus>();
    }

    [Function("UpdateAnimeStatus")]
   
    public async Task<UpdateAnimeStatusMessages> Run(
        [QueueTrigger(QueueNames.TitleProcess, Connection = "AzureWebJobsStorage")] string processResult
        )
    {
        if (processResult == ProcessResult.Ok)
        {
            _logger.LogInformation("Titles source has been updated. Verifying whether series need to be marked as completed");

            var result = await _mediator.Send(new UpdateStatusCmd());
            return result.Match(
                v =>
                {
                    return new UpdateAnimeStatusMessages
                    {
                        AnimeMessages = v.Select(Serializer.ToJson),
                        AutoSubscribeMessages = ProcessResult.Ok
                    };
                },
                e =>
                {
                    _logger.LogError("[{ArgCorrelationId}]: {ArgMessage}", e.CorrelationId, e.Message);
                    return new UpdateAnimeStatusMessages
                    {
                        AutoSubscribeMessages =  ProcessResult.Failure
                    };
                });
        }

        _logger.LogInformation("Title process failed, series status is not going to be updated");
        return  new UpdateAnimeStatusMessages
        {
            AutoSubscribeMessages =  ProcessResult.NoChanges
        };
    }

}