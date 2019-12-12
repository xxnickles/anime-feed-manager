using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions
{
    public static class UpdateMostRecentSeason
    {
        [FunctionName("UpdateMostRecentSeason")]
        [return: Table("MostRecentSeason")]
        public static SeasonStorage Run(
            [QueueTrigger("recent-season", Connection = "AzureWebJobsStorage")]
            SeasonInfo seasonInfo,
            ILogger log)
        {
            var result = new SeasonStorage
            {
                PartitionKey = "Season",
                RowKey = "Recent",
                Season = seasonInfo.Season,
                Year = seasonInfo.Year

            }.AddEtag();

            log.LogInformation($"Updating most recent season with {seasonInfo.Season} on {seasonInfo.Year}");

            return result;
        }
    }
}
