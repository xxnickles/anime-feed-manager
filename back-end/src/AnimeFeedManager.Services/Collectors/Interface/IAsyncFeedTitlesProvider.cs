using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Services.Collectors.Interface
{
    public interface IAsyncFeedTitlesProvider
    {
        Task<Either<DomainError, ImmutableList<string>>> GetTitles();
    }
}