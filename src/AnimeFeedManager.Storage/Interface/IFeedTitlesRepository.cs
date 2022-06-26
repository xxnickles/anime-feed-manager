using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Storage.Interface;

public interface IFeedTitlesRepository
{
    Task<Either<DomainError, ImmutableList<string>>> GetTitles();
    Task<Either<DomainError, Unit>> MergeTitles(IEnumerable<string> titles);
}