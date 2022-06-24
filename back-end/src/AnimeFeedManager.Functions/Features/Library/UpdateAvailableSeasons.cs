using System.Text.Json;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public static class UpdateAvailableSeasons
{
    [FunctionName("UpdateAvailableSeasons")]
    [return: Table("AvailableSeasons")]
    public static string Run(
        [QueueTrigger(QueueNames.AvailableSeasons, Connection = "AzureWebJobsStorage")]
        SeasonInfo seasonInfo, 
        ILogger log)
    {
        var result = new SeasonStorage
        {
            PartitionKey = "Season",
            RowKey = $"{seasonInfo.Year.ToString()}-{seasonInfo.Season}",
            Season = seasonInfo.Season,
            Year = seasonInfo.Year

        }.AddEtag();

        log.LogInformation("Updating available seasons with {SeasonInfoSeason} on {SeasonInfoYear}", seasonInfo.Season, seasonInfo.Year);

        return JsonSerializer.Serialize(result);
    }
}