using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Services.Collectors.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Subscriptions.Queries
{
    public class SubscribeToInterestHandler : IRequestHandler<SubscribeToInterest, Either<DomainError, IEnumerable<InterestedToSubscription>>>
    {
        private readonly IAsyncFeedTitlesProvider _feedTitles;

        public SubscribeToInterestHandler(IAsyncFeedTitlesProvider feedTitles)
        {
            _feedTitles = feedTitles;
        }

        private async Task<IEnumerable<string>> GetFeedTitles()
        {
            var titles = await _feedTitles.GetTitles();
            return titles.Match(
                x => x,
                _ => ImmutableList<string>.Empty
            );
        }

        public Task<Either<DomainError, IEnumerable<InterestedToSubscription>>> Handle(SubscribeToInterest request, CancellationToken cancellationToken)
        {
            return _feedTitles.GetTitles()
                .MapAsync(titles => GetItemsToSubscribe(titles, request.InterestedSeriesSubscription));

            
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
