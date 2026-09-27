using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Storage;
using Raffinert.FuzzySharp;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class Utils
{
    extension(ImmutableArray<FeedData> feedInfo)
    {
        /// <summary>
        /// Matches the main title first, then the alternative titles; the first hit wins.
        /// </summary>
        internal FeedData? TryGetFeedMatch(string? title,
            AlternativeTitlesData alternativeTitles)
        {
            string?[] candidates = [title, .. alternativeTitles.ForMatching];
            return candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Select(candidate => feedInfo.TryGetFeedMatch(candidate!))
                .FirstOrDefault(match => match is not null);
        }

        private FeedData? TryGetFeedMatch(string animeTitle)
        {
            return feedInfo.IsEmpty
                ? null
                : feedInfo.FirstOrDefault(info => Fuzz.WeightedRatio(info.Title, animeTitle) > 73);
        }
    }
}
