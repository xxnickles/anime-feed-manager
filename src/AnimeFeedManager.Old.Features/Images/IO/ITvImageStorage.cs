using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.State.Types;

namespace AnimeFeedManager.Old.Features.Images.IO;

public interface ITvImageStorage
{
    Task<Either<DomainError, Unit>> AddTvImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}