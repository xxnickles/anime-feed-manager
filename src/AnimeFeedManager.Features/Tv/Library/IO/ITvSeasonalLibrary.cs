using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Library.IO;

public interface ITvSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}