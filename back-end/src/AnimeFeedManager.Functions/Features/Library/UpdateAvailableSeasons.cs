using System.Text.Json;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class UpdateAvailableSeasons
{
    private readonly ILogger<UpdateAvailableSeasons> _logger;

    public UpdateAvailableSeasons(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<UpdateAvailableSeasons>();
    }

    [Function("UpdateAvailableSeasons")]
   
    public string Run(
        [QueueTrigger(QueueNames.AvailableSeasons, Connection = "AzureWebJobsStorage")]
        SeasonInfo seasonInfo)
    {
        var result = new SeasonStorage
        {
            PartitionKey = "Season",
            RowKey = $"{seasonInfo.Year}-{seasonInfo.Season}",
            Season = seasonInfo.Season,
            Year = seasonInfo.Year

        }.AddEtag();

        _logger.LogInformation("Updating available seasons with {SeasonInfoSeason} on {SeasonInfoYear}", seasonInfo.Season, seasonInfo.Year);

        return JsonSerializer.Serialize(result);
    }
}