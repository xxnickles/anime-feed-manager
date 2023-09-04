using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public sealed class InterestedToSubscribe
{
    private readonly ICreateState _createState;
    private readonly IDomainPostman _domainPostman;
    private readonly ITittlesGetter _tittlesGetter;
    private readonly IGetInterestedSeries _interestedSeriesGetter;

    public InterestedToSubscribe(
        ICreateState createState,
        IDomainPostman domainPostman,
        ITittlesGetter tittlesGetter,
        IGetInterestedSeries interestedSeriesGetter)
    {
        _createState = createState;
        _domainPostman = domainPostman;
        _tittlesGetter = tittlesGetter;
        _interestedSeriesGetter = interestedSeriesGetter;
    }

    public Task<Either<DomainError, int>> ProcessInterested(UserId userId, CancellationToken token)
    {
        return _interestedSeriesGetter.Get(userId, token)
            .BindAsync(interested => GetSeriesToSubscribe(interested, userId, token))
            .BindAsync(events => ProcessEvents(events, token));
    }

    private Task<Either<DomainError, ImmutableList<InterestedToSubscription>>> GetSeriesToSubscribe(
        ImmutableList<InterestedStorage> interestedSeries,
        UserId userId,
        CancellationToken token)
    {
        return _tittlesGetter.GetTitles(token)
            .MapAsync(titles => interestedSeries.ConvertAll(interested => (
                    new
                    {
                        FeedTitle = Utils.TryGetFeedTitle(titles, interested.RowKey ?? string.Empty),
                        InterestedTitle = interested.RowKey ?? string.Empty
                    }))
                .Where(x => !string.IsNullOrEmpty(x.FeedTitle))
                .ToImmutableList()
            )
            .MapAsync(data => data.ConvertAll(item =>
                new InterestedToSubscription(userId, item.FeedTitle, item.InterestedTitle)));
    }

    private Task<Either<DomainError, int>> ProcessEvents(ImmutableList<InterestedToSubscription> events,
        CancellationToken token)
    {
        return _createState.Create(NotificationTarget.Tv, events)
            .BindAsync(stateEvents => SendMessages(stateEvents, token));
    }

    private async Task<Either<DomainError, int>> SendMessages(
        ImmutableList<StateWrap<InterestedToSubscription>> events, CancellationToken token)
    {
        return await Task.WhenAll(events.AsParallel()
            .Select(imageEvent => _domainPostman.SendMessage(imageEvent, Box.AutoSubscriptionsProcess, token)))
            .Flatten()
            .MapAsync(results => results.Count);
    }
}