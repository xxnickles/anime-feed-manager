using AnimeFeedManager.Features.Domain.Events;

namespace AnimeFeedManager.Features.Images.IO;

public interface IOvasImageStorage
{
    Task<Either<DomainError, Unit>> AddOvasImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}