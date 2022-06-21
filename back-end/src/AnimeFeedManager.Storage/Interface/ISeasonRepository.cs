using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace AnimeFeedManager.Storage.Interface;

public interface ISeasonRepository
{
    Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons();
}