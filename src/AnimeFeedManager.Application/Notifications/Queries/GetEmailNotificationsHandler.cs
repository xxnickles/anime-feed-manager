using System.Collections.Immutable;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;
using FeedInfo = AnimeFeedManager.Core.Domain.FeedInfo;

namespace AnimeFeedManager.Application.Notifications.Queries;

public sealed record GetEmailNotificationsQry
    (ImmutableList<SubscriptionCollection> SubscriptionsToProcess) : IRequest<
        Either<DomainError, ImmutableList<Notification>>>;

public class GetEmailNotificationsHandler
    : IRequestHandler<GetEmailNotificationsQry, Either<DomainError, ImmutableList<Notification>>>
{
    private readonly IFeedProvider _feedProvider;

    public GetEmailNotificationsHandler(IFeedProvider feedProvider)
    {
        _feedProvider = feedProvider;
    }

    public Task<Either<DomainError, ImmutableList<Notification>>> Handle(GetEmailNotificationsQry request,
        CancellationToken cancellationToken)
    {
        var process = _feedProvider.GetFeed(Resolution.Hd)
            .Map(feed => ProcessFeed(request.SubscriptionsToProcess, feed));
        return Task.FromResult(process);
    }

    private ImmutableList<Notification> ProcessFeed(ImmutableList<SubscriptionCollection> subscriptionsToProcess,
        ImmutableList<FeedInfo> feed)
    {
        return subscriptionsToProcess
            .ConvertAll(i => CreateNotification(i, feed))
            .RemoveAll(i => !i.Feeds.Any());
    }

    private static Notification CreateNotification(SubscriptionCollection subscription, IEnumerable<FeedInfo> feed)
    {
        var matchingFeeds = feed
            .Where(f => Filter(f, subscription.Series))
            .Select(
                x => new SubscribedFeed(
                    x.AnimeTitle.Value.UnpackOption(string.Empty),
                    x.Links.ToArray(),
                    x.EpisodeInfo,
                    x.PublicationDate));

        return new Notification(subscription.Subscriber, matchingFeeds);
    }

    private static bool Filter(FeedInfo f, IEnumerable<string> subscribedAnimes)
    {
        var unpackedTitle = f.AnimeTitle.Value.UnpackOption(string.Empty);
        return subscribedAnimes.Contains(unpackedTitle);
    }
}