using FuzzySharp;

namespace AnimeFeedManager.Services;

public static class Helpers
{
    public static string TryGetFeedTitle(IEnumerable<string> titleList, string animeTitle)
    {
        if (!titleList.Any()) return string.Empty;
        var result = Process.ExtractOne(animeTitle, titleList);
        return result.Score switch
        {
            var s when s > 70 => result.Value,
            _ => string.Empty
        };
    }
}