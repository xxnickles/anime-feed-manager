using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.IO;

public interface IOvasStorage
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<OvaStorage> series, CancellationToken token);
}