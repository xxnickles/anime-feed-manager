using AnimeFeedManager.Features.Seasons.Types;

namespace AnimeFeedManager.Features.Seasons.IO;

public interface ISeasonStore
{
    public Task<Either<DomainError, Unit>> AddSeason(SeasonStorage season, CancellationToken token);
}