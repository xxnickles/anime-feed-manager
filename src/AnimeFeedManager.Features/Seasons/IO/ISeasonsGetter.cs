using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ISeasonsGetter
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons(CancellationToken token);
}