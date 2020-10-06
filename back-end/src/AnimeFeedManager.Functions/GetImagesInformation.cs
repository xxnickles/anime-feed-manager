using System.Collections.Immutable;
using System.Linq;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Core.ConstrainedTypes;
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
            [Queue(QueueNames.ImageProcess)] IAsyncCollector<BlobImageInfo> queueCollector,
            ILogger log)
        {
            var deserializeImageProcess = JsonConvert.DeserializeObject<ImageProcessInfo>(contents);
            if (deserializeImageProcess?.SeasonInfo == null 
                || string.IsNullOrEmpty(deserializeImageProcess.SeasonInfo.Season)) return;

            var season = Season.FromString(deserializeImageProcess.SeasonInfo.Season);
            var data = deserializeImageProcess.ImagesInfo
                .Where(x => !string.IsNullOrEmpty(x.Title) && !string.IsNullOrEmpty(x.Url))
                .Select(x => CreateDomainInformation(x, season, deserializeImageProcess.SeasonInfo.Year))
                .ToImmutableList();

            QueueStorage.StoreInQueue(data, queueCollector, log, blobInfo => $"New image '{blobInfo.BlobName}' has been enqueue for upload. Source {blobInfo.RemoteUrl}");
        }

        private static BlobImageInfo CreateDomainInformation(ImageInfo imageInfo, Season season, int year)
        {
            var title = imageInfo.Title ?? string.Empty;
            var partition = IdHelpers.GenerateAnimePartitionKey(season, (ushort)year);
            var id = IdHelpers.GenerateAnimeId(season.ToString(), year.ToString(), title);
            var directory = $"{year.ToString()}/{season.Value}";
         
            var blobName = IdHelpers.CleanAndFormatAnimeTitle(title);
            return new BlobImageInfo(partition, id, directory, blobName, imageInfo.Url ?? string.Empty);
        }
    }
}
