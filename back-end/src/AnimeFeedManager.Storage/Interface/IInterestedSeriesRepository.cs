using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace AnimeFeedManager.Storage.Interface
{
    public interface IInterestedSeriesRepository
    {
        Task<Either<DomainError, IImmutableList<InterestedStorage>>> GetAll();
        Task<Either<DomainError, IEnumerable<InterestedStorage>>> Get(Email userEmail);
        Task<Either<DomainError, InterestedStorage>> Merge(InterestedStorage subscription);
        Task<Either<DomainError, Unit>> Delete(InterestedStorage subscription);
    }
}