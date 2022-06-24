using System.IO;
using System.Net;
using System.Threading.Tasks;
using AnimeFeedManager.Application.AnimeLibrary.Commands;
using AnimeFeedManager.Functions.Models;
using AnimeFeedManager.Storage.Domain;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class UploadImage
{
    private readonly IMediator _mediator;

    public UploadImage(IMediator mediator) =>
        _mediator = mediator;
        

    [FunctionName("UploadImage")]
    [StorageAccount("AzureWebJobsStorage")]
    public async Task Run(
        [QueueTrigger(QueueNames.ImageProcess, Connection = "AzureWebJobsStorage")]
        BlobImageInfo imageInfo,
        [Blob("anime-library", FileAccess.Write)]
        CloudBlobContainer imagesContainer,
        ILogger log)
    { 
        log.LogInformation($"Getting image for {imageInfo.BlobName} from {imageInfo.RemoteUrl}");
        await imagesContainer.CreateIfNotExistsAsync();
        // Set container Access in case is not set
        await SetContainerAccess(imagesContainer);
        // Simulate to be a browser. This avoids 403/418(lol) responses
        var webClient = new WebClient();
        webClient.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        webClient.Headers.Add("Content-Type", "application / zip, application / octet - stream");
        webClient.Headers.Add("Accept-Encoding", "gzip,deflate,sdch");
        //webClient.Headers.Add("Referer", "https://google.com");
        webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

        using MemoryStream stream = new MemoryStream(webClient.DownloadData(imageInfo.RemoteUrl));
        // Sets basic blob metadata
        var blob = imagesContainer.GetBlockBlobReference($"{imageInfo.Directory}/{imageInfo.BlobName}.jpg");
        blob.Properties.ContentType = "image/jpg";

        // Upload stream to blob container
        await blob.UploadFromStreamAsync(stream);
        log.LogInformation($"{imageInfo.BlobName} has been uploaded");

        // Update AnimeInfo
        var imageStorage = new ImageStorage
        {
            ImageUrl = blob.Uri.AbsoluteUri,
            PartitionKey = imageInfo.Partition,
            RowKey = imageInfo.Id
        }.AddEtag();

        await UpdateAnimeInfo(imageStorage, log);

    }

    private async Task UpdateAnimeInfo(ImageStorage imageStorage, ILogger log)
    {
        var result =  await _mediator.Send(new AddImageUrl(imageStorage));
        result.Match(
            _ => log.LogInformation($"{imageStorage.RowKey} has been updated"),
            e => log.LogError($"[{e.CorrelationId}]: {e.Message}")
        );
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