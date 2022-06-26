using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface ISeasonRepository
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons();
    Task<Either<DomainError, Unit>> Merge(SeasonStorage seasonStorage);
}