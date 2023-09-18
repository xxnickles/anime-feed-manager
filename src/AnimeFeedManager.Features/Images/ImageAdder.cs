using System.Diagnostics;
using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Events;
using AnimeFeedManager.Features.Images.IO;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Images;

public class ImageAdder
{
    private readonly IImagesBlobStore _imagesBlobStore;
    private readonly ITvImageStorage _tvImageStorage;
    private readonly IOvasImageStorage _ovasImageStorage;
    private readonly IMoviesImageStorage _moviesImageStorage;
    private readonly ILogger<ImageAdder> _logger;

    public ImageAdder(
        IImagesBlobStore imagesBlobStore,
        ITvImageStorage tvImageStorage,
        IOvasImageStorage ovasImageStorage,
        IMoviesImageStorage moviesImageStorage,
        ILogger<ImageAdder> logger)
    {
        _imagesBlobStore = imagesBlobStore;
        _tvImageStorage = tvImageStorage;
        _ovasImageStorage = ovasImageStorage;
        _moviesImageStorage = moviesImageStorage;
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
            SeriesType.Movie => _moviesImageStorage.AddMoviesImage(stateWrap,imageUrl,token),
            SeriesType.Ova =>  _ovasImageStorage.AddOvasImage(stateWrap,imageUrl,token),
            SeriesType.None => throw new UnreachableException(),
            _ => throw new UnreachableException()
        };
    }
}