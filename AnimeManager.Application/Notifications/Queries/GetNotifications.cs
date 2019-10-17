using AnimeFeedManager.Application.Subscriptions;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Notifications.Queries
{
    public  sealed class GetNotifications : IRequest<Either<DomainError, ImmutableList<Notification>>>
    {
        //public Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> Subscriptions { get; }
        //public Task<Either<DomainError, ImmutableList<FeedInfo>>> Feed { get; }

        //public GetNotifications(Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> subscriptions,
        //    Task<Either<DomainError, ImmutableList<FeedInfo>>> feed)
        //{
        //    Subscriptions = subscriptions;
        //    Feed = feed;
        //}
    }
}
