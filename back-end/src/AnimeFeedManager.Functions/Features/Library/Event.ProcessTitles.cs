using System;
using System.Text.Json;
using System.Threading.Tasks;
using AnimeFeedManager.Application.Feed.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class ProcessTitles
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProcessTitles> _logger;

    public ProcessTitles(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<ProcessTitles>();
    }

    [Function("ProcessTitles")]
    [QueueOutput(QueueNames.TitleProcess, Connection = "AzureWebJobsStorage")]
    public async Task<string> Run(
        [BlobTrigger("feed-titles-process/{name}", Connection = "AzureWebJobsStorage")] string contents,
        string name)
    {
        var command = JsonSerializer.Deserialize<AddTitlesCmd>(contents);
        ArgumentNullException.ThrowIfNull(command);
        var result = await _mediator.Send(command);
        return result.Match(
            _ =>
            {
                _logger.LogInformation("Titles have been updated");
                return ProcessResult.Ok;
            },
            e =>
            {
                _logger.LogError("An error occurred while storing feed titles {S}", e.ToString());
                return ProcessResult.Failure;
            });
    }
}