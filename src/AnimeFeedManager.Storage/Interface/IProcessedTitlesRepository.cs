using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IProcessedTitlesRepository
{
    Task<Either<DomainError, ImmutableList<string>>> GetProcessedTitlesForSubscriber(string subscriber);
    Task<Either<DomainError, Unit>> RemoveExpired();
    Task<Either<DomainError, Unit>> Merge(ProcessedTitlesStorage processedTitles);
}