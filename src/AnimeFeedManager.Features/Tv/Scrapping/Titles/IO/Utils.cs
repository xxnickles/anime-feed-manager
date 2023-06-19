namespace AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

internal static class Utils
{
    private const char Comma = ',';
    private const char CommaReplacement = '#';

    internal static IEnumerable<string> ReplaceTitleCommas(IEnumerable<string> source)
    {
        return source.Select(t => t.Contains(Comma) ? t.Replace(Comma, CommaReplacement) : t);
    }

    private static IEnumerable<string> RestoreTitleCommas(IEnumerable<string> source)
    {
        return source.Select(t => t.Contains(Comma) ? t.Replace(CommaReplacement, Comma) : t);
    }
}