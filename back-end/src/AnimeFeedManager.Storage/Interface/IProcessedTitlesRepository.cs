using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface;

public interface IProcessedTitlesRepository
{
    Task<Either<DomainError, IImmutableList<string>>> GetProcessedTitles();
    Task<Either<DomainError, Unit>> RemoveExpired();
}