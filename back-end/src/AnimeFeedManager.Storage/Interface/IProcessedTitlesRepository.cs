using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface;

public interface IProcessedTitlesRepository
{
    Task<Either<DomainError, ImmutableList<string>>> GetProcessedTitles();
    Task<Either<DomainError, Unit>> RemoveExpired();
    
    Task<Either<DomainError, Unit>> Merge(ProcessedTitlesStorage processedTitles);
}