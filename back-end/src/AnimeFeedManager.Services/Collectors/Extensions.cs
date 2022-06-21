namespace AnimeFeedManager.Services.Collectors;

internal static class Extensions
{

    internal static string ReplaceKnownProblematicCharacters(this string @this)
    {
        return @this.Replace('–', '-').Replace('-','-');
    }
}