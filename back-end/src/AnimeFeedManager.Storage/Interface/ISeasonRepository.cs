using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeFeedManager.Storage.Interface
{
    public interface ISeasonRepository
    {
        Task<Either<DomainError, IEnumerable<SeasonStorage>>> GetAvailableSeasons();
    }
}