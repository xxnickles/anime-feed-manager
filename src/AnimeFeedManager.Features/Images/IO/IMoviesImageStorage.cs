using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;

namespace AnimeFeedManager.Features.Images.IO;

public interface IMoviesImageStorage
{
    Task<Either<DomainError, Unit>> AddMoviesImage(
        StateWrap<DownloadImageEvent> imageStateWrap, 
        string imageUrl,
        CancellationToken token);
}