using AnimeFeedManager.Features.Domain.Events;
using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.Images.IO;

public interface IMoviesImageStorage
{
    Task<Either<DomainError, Unit>> AddMoviesImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}