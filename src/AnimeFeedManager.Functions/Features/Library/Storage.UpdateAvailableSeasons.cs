using AnimeFeedManager.Application.Seasons.Commands;
using AnimeFeedManager.Functions.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class UpdateAvailableSeasons
{
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateAvailableSeasons> _logger;

    public UpdateAvailableSeasons(IMediator mediator, ILoggerFactory loggerFactory)
    {
        _mediator = mediator;
        _logger = loggerFactory.CreateLogger<UpdateAvailableSeasons>();
    }

    [Function("UpdateAvailableSeasons")]
   
    public async Task Run(
        [QueueTrigger(QueueNames.AvailableSeasons, Connection = "AzureWebJobsStorage")]
        SeasonInfo seasonInfo)
    {

        _logger.LogInformation("Updating available seasons with {SeasonInfoSeason} on {SeasonInfoYear}", seasonInfo.Season, seasonInfo.Year);
        var command = new MergeSeasonHandlerCmd(new SeasonStorage
        {
            PartitionKey = "Season",
            RowKey = $"{seasonInfo.Year}-{seasonInfo.Season}",
            Season = seasonInfo.Season,
            Year = seasonInfo.Year

        });
        var result = await _mediator.Send(command);
        result.Match(
            _ => _logger.LogInformation("{SeasonInfoSeason} on {SeasonInfoYear} has been stored", seasonInfo.Season, seasonInfo.Year),
            e => _logger.LogError("[{CorrelationId}]: {Message}", e.CorrelationId, e.Message)
        );
    }
}