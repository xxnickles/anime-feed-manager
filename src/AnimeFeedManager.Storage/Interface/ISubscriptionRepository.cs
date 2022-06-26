using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface ISubscriptionRepository
{
    Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> Get(Email userEmail);
    Task<Either<DomainError, ImmutableList<SubscriptionStorage>>> GetAll();
    Task<Either<DomainError, Unit>> Merge(SubscriptionStorage subscription);
    Task<Either<DomainError, Unit>> Delete(SubscriptionStorage subscription);
}