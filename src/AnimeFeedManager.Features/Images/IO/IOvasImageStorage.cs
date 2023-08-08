using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.Images.IO;

public interface IOvasImageStorage
{
    Task<Either<DomainError, Unit>> AddOvasImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}