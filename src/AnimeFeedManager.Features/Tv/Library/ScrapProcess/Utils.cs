using AnimeFeedManager.Features.Scrapping.Types;
using Raffinert.FuzzySharp;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class Utils
{
    internal static FeedData? TryGetFeedMatch(this ImmutableArray<FeedData> feedInfo, string animeTitle)
    {
        return feedInfo.IsEmpty
            ? null
            : feedInfo.FirstOrDefault(info => Fuzz.WeightedRatio(info.Title, animeTitle) > 73);
    }
}