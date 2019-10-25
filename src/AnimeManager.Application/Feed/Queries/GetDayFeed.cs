using AnimeFeedManager.Application.AnimeLibrary.Queries;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;

namespace AnimeFeedManager.Application.Feed.Queries
{
    public sealed class GetDayFeed : Record<GetExternalLibrary>, IRequest<Either<DomainError, ImmutableList<FeedInfo>>>
    {
        public Resolution Resolution { get; }
        public GetDayFeed(Resolution resolution) => Resolution = resolution;
    }
}
