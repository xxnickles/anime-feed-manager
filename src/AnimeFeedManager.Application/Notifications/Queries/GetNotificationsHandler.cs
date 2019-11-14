using AnimeFeedManager.Application.Feed.Queries;
using AnimeFeedManager.Application.Subscriptions;
using AnimeFeedManager.Application.Subscriptions.Queries;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using LanguageExt;
using MediatR;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Notifications.Queries
{
    public class GetNotificationsHandler : IRequestHandler<GetNotifications, Either<DomainError, ImmutableList<Notification>>>
    {
        private readonly IMediator _mediator;

        public GetNotificationsHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task<Either<DomainError, ImmutableList<Notification>>> Handle(GetNotifications request, CancellationToken cancellationToken)
        {
          return _mediator.Send(new GetDayFeed(Resolution.Hd), cancellationToken)
                .BindAsync(ProcessFeed);
        }

        public Task<Either<DomainError, ImmutableList<Notification>>> ProcessFeed(ImmutableList<FeedInfo> feed)
        {
           return _mediator.Send(new GetSubscriptions())
                .MapAsync(s => s.ConvertAll(i => CreateNotification(i, feed)))
                .MapAsync(n => n.RemoveAll(i => !i.Feeds.Any()));
        }

        private static Notification CreateNotification(SubscriptionCollection subscription, IEnumerable<FeedInfo> feed)
        {
            var matchingFeeds = feed
                .Where(f => Filter(f, subscription.SubscribedAnimes))
                .Select(
                    x => new SubscribedFeed(OptionUtils.UnpackOption(x.AnimeTitle.Value, string.Empty),
                        x.Link,
                        x.PublicationDate));

            return new Notification(subscription.SubscriptionId, matchingFeeds);
        }

        private static bool Filter(FeedInfo f, IEnumerable<string> subscribedAnimes)
        {
            var unpackedTitle = OptionUtils.UnpackOption(f.AnimeTitle.Value, string.Empty);
            return subscribedAnimes.Contains(unpackedTitle);
        }
    }
}
