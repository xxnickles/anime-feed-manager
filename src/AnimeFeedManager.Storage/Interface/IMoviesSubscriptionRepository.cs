using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IMoviesSubscriptionRepository
{
    Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> Get(Email userEmail);
    Task<Either<DomainError, ImmutableList<MoviesSubscriptionStorage>>> GetTodaySubscriptions();
    Task<Either<DomainError, Unit>> Complete(string subscriber, string title);
    Task<Either<DomainError, Unit>> Merge(MoviesSubscriptionStorage subscription);
    Task<Either<DomainError, Unit>> Delete(string subscriber, string title);
}