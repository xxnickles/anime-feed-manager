using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface;

public interface ISeasonRepository
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons();
    Task<Either<DomainError, Unit>> Merge(SeasonStorage seasonStorage);
}