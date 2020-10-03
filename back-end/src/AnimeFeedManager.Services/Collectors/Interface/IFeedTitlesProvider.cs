using System.Collections.Immutable;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Services.Collectors.Interface
{
    public interface IFeedTitlesProvider
    {
        Either<DomainError, ImmutableList<string>> GetTitles();
    }
}