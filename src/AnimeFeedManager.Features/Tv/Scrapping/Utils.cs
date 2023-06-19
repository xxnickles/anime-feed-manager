using FuzzySharp;

namespace AnimeFeedManager.Features.Tv.Scrapping;

internal static class Utils
{
    internal static string TryGetFeedTitle(IEnumerable<string> titleList, string animeTitle)
    {
        if (!titleList.Any()) return string.Empty;
        var result = Process.ExtractOne(animeTitle, titleList);
        return result.Score switch
        {
            > 70 => result.Value,
            _ => string.Empty
        };
    }
}