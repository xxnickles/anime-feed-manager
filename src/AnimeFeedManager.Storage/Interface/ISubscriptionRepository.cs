using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface
{
    public interface ISubscriptionRepository
    {
        Task<Either<DomainError, IEnumerable<SubscriptionStorage>>> Get(Email userEmail);
        Task<Either<DomainError, IEnumerable<SubscriptionStorage>>> GetAll();
        Task<Either<DomainError, SubscriptionStorage>> Merge(SubscriptionStorage subscription);
    }
}