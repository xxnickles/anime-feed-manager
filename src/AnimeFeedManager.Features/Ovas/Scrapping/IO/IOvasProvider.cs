using AnimeFeedManager.Features.Ovas.Scrapping.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.IO;

public interface IOvasProvider
{
    Task<Either<DomainError, OvasCollection>> GetLibrary(SeasonSelector season, CancellationToken token);
}