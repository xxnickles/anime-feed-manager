using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
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

    public UpdateAnimeStatus(IMediator mediator) => _mediator = mediator;

    [FunctionName("UpdateAnimeStatus")]
    [StorageAccount("AzureWebJobsStorage")]
   
    public async Task<UpdateAnimeStatusMessages> Run(
        [QueueTrigger(QueueNames.TitleProcess)] string processResult,
        ILogger log)
    {
        if (processResult == ProcessResult.Ok)
        {
            log.LogInformation("Titles source has been updated. Verifying whether series need to be marked as completed");

            var result = await _mediator.Send(new UpdateStatus());
            return result.Match(
                v =>
                {
                    return new UpdateAnimeStatusMessages
                    {
                        AnimeMessages = v.Select(a => JsonSerializer.Serialize(a)),
                        AutoSubscribeMessages = ProcessResult.Ok
                    };
                },
                e =>
                {
                    log.LogError("[{ArgCorrelationId}]: {ArgMessage}", e.CorrelationId, e.Message);
                    return new UpdateAnimeStatusMessages
                    {
                        AutoSubscribeMessages =  ProcessResult.Failure
                    };
                });
        }

        log.LogInformation("Title process failed, series status is not going to be updated");
        return  new UpdateAnimeStatusMessages
        {
            AutoSubscribeMessages =  ProcessResult.NoChanges
        };
    }

}