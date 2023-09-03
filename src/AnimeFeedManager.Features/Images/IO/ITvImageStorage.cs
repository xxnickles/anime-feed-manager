using AnimeFeedManager.Features.Domain.Events;

namespace AnimeFeedManager.Features.Images.IO;

public interface ITvImageStorage
{
    Task<Either<DomainError, Unit>> AddTvImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}