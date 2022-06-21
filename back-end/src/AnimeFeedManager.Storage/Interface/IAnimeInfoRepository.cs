using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface;

public interface IAnimeInfoRepository
{
    Task<Either<DomainError, IEnumerable<AnimeInfoWithImageStorage>>> GetBySeason(Season season, int year);
    Task<Either<DomainError, IEnumerable<AnimeInfoWithImageStorage>>> GetIncomplete();
    Task<Either<DomainError, IEnumerable<AnimeInfoStorage>>> GetAll();
    Task<Either<DomainError, Unit>> Merge(AnimeInfoStorage animeInfo);
    Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image);
}