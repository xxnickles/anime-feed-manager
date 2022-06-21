using AnimeFeedManager.Application.Feed.Commands;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Functions.Models;

namespace AnimeFeedManager.Functions.Features.Library;

public class ProcessTitles
{
    private readonly IMediator _mediator;

    public ProcessTitles(IMediator mediator)
    {
        _mediator = mediator;
    }

    [FunctionName("ProcessTitles")]
    [return: Queue(QueueNames.TitleProcess, Connection = "AzureWebJobsStorage")]
    public async Task<string> Run(
        [BlobTrigger("feed-titles-process/{name}", Connection = "AzureWebJobsStorage")] string contents,
        string name,
        ILogger log)
    {
        var command = JsonConvert.DeserializeObject<AddTitles>(contents);

        var result = await _mediator.Send(command);
        return result.Match(
            _ =>
            {
                log.LogInformation("Titles have been updated");
                return ProcessResult.Ok;
            },
            e =>
            {
                log.LogError($"An error occurred while storing feed titles {e.ToString()}");
                return ProcessResult.Failure;
            });
    }
}