using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.State.Types;

namespace AnimeFeedManager.Old.Features.Images.IO;

public interface IOvasImageStorage
{
    Task<Either<DomainError, Unit>> AddOvasImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}