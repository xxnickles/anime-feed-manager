using System.Collections.Immutable;
using AnimeFeedManager.Application.Subscriptions;
using AnimeFeedManager.Application.Subscriptions.Queries;
using AnimeFeedManager.Core.Utils;
using MediatR;

namespace AnimeFeedManager.Application.Notifications.Queries;

public  sealed record GetNotificationsQry(ImmutableList<FeedInfo> Feed) : IRequest<Either<DomainError, ImmutableList<Notification>>>;

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQry, Either<DomainError, ImmutableList<Notification>>>
{
    private readonly IMediator _mediator;

    public GetNotificationsHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<Either<DomainError, ImmutableList<Notification>>> Handle(GetNotificationsQry request, CancellationToken cancellationToken)
    {
        return ProcessFeed(request.Feed);
    }

    public Task<Either<DomainError, ImmutableList<Notification>>> ProcessFeed(ImmutableList<FeedInfo> feed)
    {
        return _mediator.Send(new GetSubscriptionsQry())
            .MapAsync(s => s.ConvertAll(i => CreateNotification(i, feed)))
            .MapAsync(n => n.RemoveAll(i => !i.Feeds.Any()));
    }

    private static Notification CreateNotification(SubscriptionCollection subscription, IEnumerable<FeedInfo> feed)
    {
        var matchingFeeds = feed
            .Where(f => Filter(f, subscription.SubscribedAnimes))
            .Select(
                x => new SubscribedFeed(OptionUtils.UnpackOption(x.AnimeTitle.Value, string.Empty),
                    x.Links,
                    x.EpisodeInfo,
                    x.PublicationDate));

        return new Notification(subscription.SubscriptionId, matchingFeeds);
    }

    private static bool Filter(FeedInfo f, IEnumerable<string> subscribedAnimes)
    {
        var unpackedTitle = OptionUtils.UnpackOption(f.AnimeTitle.Value, string.Empty);
        return subscribedAnimes.Contains(unpackedTitle);
    }
}