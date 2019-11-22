using System.Collections.Immutable;
using System.Linq;
using AnimeFeedManager.Common.Extensions;
using AnimeFeedManager.Functions.Helpers;
using AnimeFeedManager.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AnimeFeedManager.Functions
{
    public static class GetImagesInformation
    {
        [FunctionName("GetImagesInformation")]
        public static void Run(
            [BlobTrigger("images-process/{name}", Connection = "AzureWebJobsStorage")]string contents, 
            string name,
            [Queue("image-process")] IAsyncCollector<BlobImageInfo> queueCollector,
            ILogger log)
        {
            var deserializeImageProcess = JsonConvert.DeserializeObject<ImageProcessInfo>(contents);
            if (deserializeImageProcess?.SeasonInfo == null || string.IsNullOrEmpty(deserializeImageProcess.SeasonInfo.Season)) return;
            var directory =
                $"{deserializeImageProcess.SeasonInfo.Year.ToString()}/{deserializeImageProcess.SeasonInfo.Season}";
            var data = deserializeImageProcess.ImagesInfo
                .Where(x => !string.IsNullOrEmpty(x.Title) && !string.IsNullOrEmpty(x.Url))
                .Select(x =>
                    new BlobImageInfo(directory, x.Title?.ToApplicationId().ToLowerInvariant() ?? string.Empty, x.Url ?? string.Empty))
                .ToImmutableList();

            QueueStorage.StoreInQueue(data, queueCollector, log, blobInfo => $"New image '{blobInfo.BlobName}' has been enqueue for upload. Source {blobInfo.RemoteUrl}");
        }
    }
}
