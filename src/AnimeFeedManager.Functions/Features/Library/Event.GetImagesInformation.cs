using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Functions.Models;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Features.Library;

public class GetImagesInformation
{
    private readonly ILogger<GetImagesInformation> _logger;

    public GetImagesInformation(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GetImagesInformation>();
    }

    [Function("GetImagesInformation")]
    [QueueOutput(QueueNames.ImageProcess, Connection = "AzureWebJobsStorage")]
    public IEnumerable<string> Run(
        [BlobTrigger("images-process/{name}", Connection = "AzureWebJobsStorage")] string contents)
    {
        
        var deserializeImageProcess = Serializer.FromJson<ImageProcessInfo>(contents);
        if (deserializeImageProcess?.SeasonInfo == null
            || string.IsNullOrEmpty(deserializeImageProcess.SeasonInfo.Season))
        {
            _logger.LogWarning("Images update blob content couldn't be deserialized");
            return Enumerable.Empty<string>();
        }
        _logger.LogInformation("Running update of the Image Information");
        var season = Season.FromString(deserializeImageProcess.SeasonInfo.Season);
        if (deserializeImageProcess.ImagesInfo != null)
            return deserializeImageProcess.ImagesInfo
                .Where(x => !string.IsNullOrEmpty(x.Title) && !string.IsNullOrEmpty(x.Url))
                .Select(x => CreateDomainInformation(x, season, deserializeImageProcess.SeasonInfo.Year))
                .Select(Serializer.ToJson);
        
        return Enumerable.Empty<string>();
    }

    private static BlobImageInfoEvent CreateDomainInformation(ImageInfo imageInfo, Season season, int year)
    {
        var title = imageInfo.Title ?? string.Empty;
        var partition = IdHelpers.GenerateAnimePartitionKey(season, (ushort)year);
        var id = IdHelpers.GenerateAnimeId(season.ToString(), year.ToString(), title);
        var directory = $"{year}/{season.Value}";

        var blobName = IdHelpers.CleanAndFormatAnimeTitle(title);
        return new BlobImageInfoEvent(partition, id, directory, blobName, imageInfo.Url ?? string.Empty);
    }
}