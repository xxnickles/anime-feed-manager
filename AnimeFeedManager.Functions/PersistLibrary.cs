using AnimeFeedManager.Storage.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions
{
    public static class PersistLibrary
    {
        [FunctionName("PersistLibrary")]
        [StorageAccount("AzureWebJobsStorage")]
        [return: Table("AnimeLibrary")]
        public static AnimeInfoStorage Run(
            [QueueTrigger("anime-library")]AnimeInfoStorage animeInfo,
            ILogger log)
        {
            log.LogInformation($"storing {animeInfo.Title}");
            return animeInfo;
        }
    }
}
