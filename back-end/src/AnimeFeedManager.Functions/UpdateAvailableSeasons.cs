using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions
{
    public static class UpdateAvailableSeasons
    {
        [FunctionName("UpdateAvailableSeasons")]
        [return: Table("AvailableSeasons")]
        public static SeasonStorage Run(
            [QueueTrigger("available-seasons", Connection = "AzureWebJobsStorage")]
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

            log.LogInformation($"Updating available seasons with {seasonInfo.Season} on {seasonInfo.Year}");

            return result;
        }
    }
}
