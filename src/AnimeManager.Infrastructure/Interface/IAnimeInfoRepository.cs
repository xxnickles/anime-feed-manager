using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface
{
    public interface IAnimeInfoRepository
    {
        Task<Either<DomainError, IEnumerable<AnimeInfoStorage>>> GetBySeason(Season season, int year);
    }
}