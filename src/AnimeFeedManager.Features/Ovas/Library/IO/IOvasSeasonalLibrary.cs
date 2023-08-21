using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library.IO;

public interface IOvasSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}