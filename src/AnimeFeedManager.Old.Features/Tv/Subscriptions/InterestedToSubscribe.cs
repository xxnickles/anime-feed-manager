﻿using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Notifications.Base;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series.Types;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Features.Tv.Subscriptions;

public sealed class InterestedToSubscribe(
    IStateCreator stateCreator,
    IDomainPostman domainPostman,
    ITittlesGetter tittlesGetter,
    IAlternativeTitlesGetter alternativeTitlesGetter,
    IGetInterestedSeries interestedSeriesGetter)
{
    public Task<Either<DomainError, int>> ProcessInterested(UserId userId, CancellationToken token)
    {
        return interestedSeriesGetter.Get(userId, token)
            .BindAsync(interested => GetSeriesToSubscribe(interested, userId, token))
            .BindAsync(events => ProcessEvents(events, token));
    }

    private Task<Either<DomainError, ImmutableList<InterestedToSubscription>>> GetSeriesToSubscribe(
        ImmutableList<InterestedStorage> interestedSeries,
        UserId userId,
        CancellationToken token)
    {
        return tittlesGetter.GetTitles(token)
            .BindAsync(titles => alternativeTitlesGetter
                .ByOriginalTitles(interestedSeries.ConvertAll(i => i.RowKey ?? string.Empty), token).MapAsync(
                    alternativeTitlesMap => new
                    {
                        titles,
                        alternativeTitlesMap
                    }))
            .MapAsync(data => interestedSeries.ConvertAll(interested => new
                {
                    FeedTitle = TryToGetFeedTitle(interested.RowKey ?? string.Empty, data.titles,
                        data.alternativeTitlesMap),
                    InterestedTitle = interested.RowKey ?? string.Empty
                })
                .Where(x => !string.IsNullOrEmpty(x.FeedTitle))
                .ToImmutableList()
            )
            .MapAsync(data => data.ConvertAll(item =>
                new InterestedToSubscription(userId, item.FeedTitle, item.InterestedTitle)));
    }

    private static string TryToGetFeedTitle(string interestedTitle, ImmutableList<string> titles,
        ImmutableList<TilesMap> alternativeTitlesMap)
    {
        var feedTitle = Utils.TryGetFeedTitle(titles, interestedTitle);
        if (!string.IsNullOrEmpty(feedTitle)) return feedTitle;
        var alternative = alternativeTitlesMap.FirstOrDefault(at => at.Original == interestedTitle).Alternative;
        return alternative is not null ? Utils.TryGetFeedTitle(titles, alternative) : feedTitle;
    }

    private Task<Either<DomainError, int>> ProcessEvents(ImmutableList<InterestedToSubscription> events,
        CancellationToken token)
    {
        return stateCreator.Create(NotificationTarget.Tv, events, new Box(InterestedToSubscription.TargetQueue))
            .BindAsync(stateEvents => SendMessages(stateEvents, token));
    }

    private async Task<Either<DomainError, int>> SendMessages(
        ImmutableList<StateWrap<InterestedToSubscription>> events, CancellationToken token)
    {
        return await Task.WhenAll(events.AsParallel()
                .Select(stateWrap => domainPostman.SendMessage(stateWrap, token)))
            .FlattenResults()
            .MapAsync(results => results.Count);
    }
}