using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IInterestedSeriesRepository
{
    Task<Either<DomainError, ImmutableList<InterestedStorage>>> GetAll();
    Task<Either<DomainError, ImmutableList<InterestedStorage>>> Get(Email userEmail);
    Task<Either<DomainError, Unit>> Merge(InterestedStorage subscription);
    Task<Either<DomainError, Unit>> Delete(InterestedStorage subscription);
}