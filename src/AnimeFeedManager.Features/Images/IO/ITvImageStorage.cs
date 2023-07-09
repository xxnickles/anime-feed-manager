using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.Images.IO;

public interface ITvImageStorage
{
    Task<Either<DomainError, Unit>> AddTvImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}