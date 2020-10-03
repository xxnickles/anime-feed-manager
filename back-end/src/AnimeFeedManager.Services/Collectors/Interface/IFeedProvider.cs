using System.Collections.Immutable;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Services.Collectors.Interface
{
    public interface IFeedProvider
    {
        Either<DomainError, ImmutableList<FeedInfo>> GetFeed(Resolution resolution);
    }
}