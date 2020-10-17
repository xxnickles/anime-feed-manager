using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Application.Notifications.Queries
{
    public  sealed class GetNotifications : IRequest<Either<DomainError, ImmutableList<Notification>>>
    {
        public GetNotifications(ImmutableList<FeedInfo> feed)
        {
            Feed = feed;
        }

        public ImmutableList<FeedInfo> Feed { get; }
    }
}
