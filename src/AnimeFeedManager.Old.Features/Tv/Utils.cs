using FuzzySharp;

namespace AnimeFeedManager.Features.Tv;

internal static class Utils
{
    internal static string TryGetFeedTitle(ImmutableList<string> titleList, string animeTitle)
    {
        if (titleList.IsEmpty) return string.Empty;
        var result = Process.ExtractOne(animeTitle, titleList);
        return result.Score switch
        {
            > 70 => result.Value,
            _ => string.Empty
        };
    }
}