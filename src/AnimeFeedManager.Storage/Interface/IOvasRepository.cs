using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IOvasRepository
{
    Task<Either<DomainError, ImmutableList<OvaStorage>>> GetBySeason(Season season, int year);
    Task<Either<DomainError, Unit>> Merge(OvaStorage animeInfo);
    Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image);
}