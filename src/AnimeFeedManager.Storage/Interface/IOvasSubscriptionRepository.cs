using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IOvasSubscriptionRepository
{
    Task<Either<DomainError, ImmutableList<OvasSubscriptionStorage>>> GetTodaySubscriptions();
    Task<Either<DomainError, Unit>> Complete(string subscriber, string title);
    Task<Either<DomainError, Unit>> Merge(OvasSubscriptionStorage subscription);
    Task<Either<DomainError, Unit>> Delete(string subscriber, string title);
}