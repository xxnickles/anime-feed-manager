using System.Collections.Immutable;

namespace AnimeFeedManager.Storage.Interface;

public interface IFeedTitlesRepository
{
    Task<Either<DomainError, ImmutableList<string>>> GetTitles();
    Task<Either<DomainError, Unit>> MergeTitles(IEnumerable<string> titles);
}