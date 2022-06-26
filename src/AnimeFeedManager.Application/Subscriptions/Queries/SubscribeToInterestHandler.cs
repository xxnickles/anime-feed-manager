using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public sealed record SubscribeToInterestQry(IImmutableList<InterestedSeriesItem> InterestedSeriesSubscription) :  IRequest<Either<DomainError, IEnumerable<InterestedToSubscription>>>;
public class SubscribeToInterestHandler : IRequestHandler<SubscribeToInterestQry, Either<DomainError, IEnumerable<InterestedToSubscription>>>
{
    private readonly IFeedTitlesRepository _feedTitles;

    public SubscribeToInterestHandler(IFeedTitlesRepository feedTitles)
    {
        _feedTitles = feedTitles;
    }

    public Task<Either<DomainError, IEnumerable<InterestedToSubscription>>> Handle(SubscribeToInterestQry request, CancellationToken cancellationToken)
    {
        return _feedTitles.GetTitles()
            .MapAsync(titles => GetItemsToSubscribe(titles, request.InterestedSeriesSubscription));
    }

    private IEnumerable<InterestedToSubscription> GetItemsToSubscribe(IEnumerable<string> feedTitles, IImmutableList<InterestedSeriesItem> interestedSeriesSubscription)
    {
        var uniqueTitles = interestedSeriesSubscription
            .Select(x => x.InterestedAnime)
            .Distinct()
            .Select(title => (feed: Helpers.TryGetFeedTitle(feedTitles, title), title))
            .Where(t => !string.IsNullOrEmpty(t.feed));

        return interestedSeriesSubscription
            .Where(i => uniqueTitles.Any(u => u.title == i.InterestedAnime))
            .Select(i => new InterestedToSubscription(i.UserId, i.InterestedAnime,
                uniqueTitles.Single(u => u.title == i.InterestedAnime).feed));
    }

      
}