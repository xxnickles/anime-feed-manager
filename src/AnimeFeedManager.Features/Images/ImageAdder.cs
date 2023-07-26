using System.Diagnostics;
using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.Images.IO;
using AnimeFeedManager.Features.State.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Images;

public class ImageAdder
{
    private readonly IImagesBlobStore _imagesBlobStore;
    private readonly ITvImageStorage _tvImageStorage;
    private readonly ILogger<ImageAdder> _logger;

    public ImageAdder(
        IImagesBlobStore imagesBlobStore,
        ITvImageStorage tvImageStorage,
        ILogger<ImageAdder> logger)
    {
        _imagesBlobStore = imagesBlobStore;
        _tvImageStorage = tvImageStorage;
        _logger = logger;
    }

    public async Task<Either<DomainError, Unit>> Add(
        Stream image,
        StateWrap<DownloadImageEvent> stateWrap, 
        CancellationToken token = default)
    {
        try
        {
            var fileLocation =
                await _imagesBlobStore.Upload($"{stateWrap.Payload.BlobName}.jpg", stateWrap.Payload.Directory,
                    image);
            _logger.LogInformation("{BlobName} has been uploaded", stateWrap.Payload.BlobName);

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
            SeriesType.Tv => _tvImageStorage.AddTvImage(stateWrap, imageUrl, token),
            SeriesType.Movie => throw new NotImplementedException(),
            SeriesType.Ova =>  throw new NotImplementedException(),
            SeriesType.None => throw new UnreachableException(),
            _ => throw new UnreachableException()
        };
    }



}