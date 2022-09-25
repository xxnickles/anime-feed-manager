using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IMoviesRepository
{
    Task<Either<DomainError, ImmutableList<MovieStorage>>> GetBySeason(Season season, int year);
    Task<Either<DomainError, Unit>> Merge(MovieStorage animeInfo);
    Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image);
}