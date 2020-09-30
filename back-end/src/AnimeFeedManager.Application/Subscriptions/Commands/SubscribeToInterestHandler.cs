using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.HorribleSubs;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public class SubscribeToInterestHandler : IRequestHandler<SubscribeToInterest, Either<DomainError, IEnumerable<InterestedToSubscription>>>
    {
        private readonly IFeedTitlesProvider _feedTitles;

        public SubscribeToInterestHandler(IFeedTitlesProvider feedTitles)
        {
            _feedTitles = feedTitles;
        }

        private IEnumerable<string> GetFeedTitles()
        {
            return _feedTitles.GetTitles().Match(
                x => x,
                _ => ImmutableList<string>.Empty
            );
        }

        public Task<Either<DomainError, IEnumerable<InterestedToSubscription>>> Handle(SubscribeToInterest request, CancellationToken cancellationToken)
        {
            var result = _feedTitles.GetTitles()
                .Map(titles => GetItemsToSubscribe(titles, request.InterestedSeriesSubscription));

            return Task.FromResult(result);
        }

        private IEnumerable<InterestedToSubscription> GetItemsToSubscribe(IEnumerable<string> feedTitles, IImmutableList<InterestedSeriesItem> interestedSeriesSubscription)
        {
            var uniqueTitles = interestedSeriesSubscription
                .Select(x => x.InterestedAnime)
                .Distinct()
                .Select(title => (feed: Services.Helpers.TryGetFeedTitle(feedTitles, title), title))
                .Where(t => !string.IsNullOrEmpty(t.feed));

            return interestedSeriesSubscription
                .Where(i => uniqueTitles.Any(u => u.title == i.InterestedAnime))
                .Select(i => new InterestedToSubscription(i.UserId, i.InterestedAnime,
                    uniqueTitles.Single(u => u.title == i.InterestedAnime).feed));
        }

      
    }
}
