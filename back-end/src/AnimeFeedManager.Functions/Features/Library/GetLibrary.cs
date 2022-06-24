using System.Collections.Generic;
using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace AnimeFeedManager.Functions.Features.Library;

public class GetLibraryMessages
{
    [QueueOutput(QueueNames.AnimeLibrary)]
    public IEnumerable<string>? AnimeMessages { get; set; }
    [QueueOutput(QueueNames.AvailableSeasons)]
    public string? SeasonMessage { get; set; }
}

public class GetLibrary
{
    private readonly IMediator _mediator;
    private readonly ILogger<GetLibraryMessages> _logger;

    public GetLibrary(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<GetLibraryMessages>();
    }

    [Function("GetLibrary")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task<GetLibraryMessages> Run(
        [TimerTrigger("0 0 2 * * SAT")] TimerInfo  timer
        )
    {
        var result = await _mediator.Send(new GetExternalLibrary());
        return result.Match(
            v =>
            {
                var seasonInfo = ExtractSeasonInformation(v.First());
                return new GetLibraryMessages
                {
                    AnimeMessages = v.Select( a => JsonSerializer.Serialize(a)),
                    SeasonMessage =  JsonSerializer.Serialize(seasonInfo)
                };
            },
            e =>
            {
                _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message);
                return new GetLibraryMessages();
            });
    }

    private SeasonInfo ExtractSeasonInformation(AnimeInfoStorage sample) => new()
    {
        Season = sample.Season,
        Year = sample.Year
    };
}