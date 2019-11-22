using AnimeFeedManager.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AnimeFeedManager.Functions
{
    public static class UploadImage
    {
        [FunctionName("UploadImage")]        
        public async static Task Run(
            [QueueTrigger("image-process", Connection = "AzureWebJobsStorage")]
            BlobImageInfo imageInfo,
            [Blob("anime-library", FileAccess.Write)]
            CloudBlobContainer imagesContainer,
            ILogger log)
        { 
            log.LogInformation($"Getting image for {imageInfo.BlobName} from {imageInfo.RemoteUrl}");
            await imagesContainer.CreateIfNotExistsAsync();
            // Set container Access in case is not set
            await SetContainerAccess(imagesContainer);
            // Simulate to be a browser. This avoids 403 responses
            var webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            webClient.Headers.Add("Content-Type", "application / zip, application / octet - stream");
            webClient.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
            webClient.Headers.Add("Referer", "https://google.com");
            webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            using MemoryStream stream = new MemoryStream(webClient.DownloadData(imageInfo.RemoteUrl));
            // Sets basic blob metadata
            var blob = imagesContainer.GetBlockBlobReference($"{imageInfo.Directory}/{imageInfo.BlobName}.jpg");
            blob.Properties.ContentType = "image/jpg";

            // Upload stream to blob container
            await blob.UploadFromStreamAsync(stream);
            log.LogInformation($"{imageInfo.BlobName} has been uploaded");

        }

        private static async Task SetContainerAccess(CloudBlobContainer container)
        {            
            BlobContainerPermissions permissions = await container.GetPermissionsAsync();
            if(permissions.PublicAccess != BlobContainerPublicAccessType.Blob)
            {
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                await container.SetPermissionsAsync(permissions);
            }
            
        }
    }
}
