using System.Diagnostics;
using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Images.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Images;

public class ImageAdder(
    IImagesBlobStore imagesBlobStore,
    ITvImageStorage tvImageStorage,
    IOvasImageStorage ovasImageStorage,
    IMoviesImageStorage moviesImageStorage,
    ILogger<ImageAdder> logger)
{
    public async Task<Either<DomainError, Unit>> Add(
        Stream image,
        StateWrap<DownloadImageEvent> stateWrap, 
        CancellationToken token = default)
    {
        try
        {
            var fileLocation =
                await imagesBlobStore.Upload($"{stateWrap.Payload.BlobName}.jpg", stateWrap.Payload.Directory,
                    image);
            logger.LogInformation("{BlobName} has been uploaded", stateWrap.Payload.BlobName);

            return await Store(stateWrap, fileLocation.AbsoluteUri, token);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
       
    }

    private  Task<Either<DomainError, Unit>> Store( StateWrap<DownloadImageEvent> stateWrap, string imageUrl, CancellationToken token)
    {
        return stateWrap.Payload.SeriesType switch
        {
            SeriesType.Tv => tvImageStorage.AddTvImage(stateWrap, imageUrl, token),
            SeriesType.Movie => moviesImageStorage.AddMoviesImage(stateWrap,imageUrl,token),
            SeriesType.Ova =>  ovasImageStorage.AddOvasImage(stateWrap,imageUrl,token),
            SeriesType.None => throw new UnreachableException(),
            _ => throw new UnreachableException()
        };
    }
}