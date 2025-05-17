using Process = FuzzySharp.Process;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class FeedTitle
{
    internal static string TryGetFeedTitle(this ImmutableList<string> titleList, string animeTitle)
    {
        if (titleList.IsEmpty) return string.Empty;
        var result = Process.ExtractOne(animeTitle, titleList);
        return result.Score switch
        {
            > 73 => result.Value,
            _ => string.Empty
        };
    }
}