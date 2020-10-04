using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Feed.Queries
{
    public class GetDayFeedHandler : IRequestHandler<GetDayFeed, Either<DomainError, ImmutableList<FeedInfo>>>
    {
        private readonly IFeedProvider _feedProvider;

        public GetDayFeedHandler(IFeedProvider feedProvider)
        {
            _feedProvider = feedProvider;
        }

        public Task<Either<DomainError, ImmutableList<FeedInfo>>> Handle(GetDayFeed request,
            CancellationToken cancellationToken) =>
           Task.FromResult(_feedProvider.GetFeed(request.Resolution));
    }
}
