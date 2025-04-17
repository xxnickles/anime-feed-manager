using System.Collections.ObjectModel;

namespace AnimeFeedManager.Old.Features;

internal static class Utils
{
    private static readonly ReadOnlyDictionary<char, string> ReplacementCharacters = new Dictionary<char, string>
    {
        {'/', ".slash."}
    }.AsReadOnly();


    internal static string ReplaceForbiddenRowKeyParameters(this RowKey value)
    {
        return value.ToString().ReplaceForbiddenRowKeyParameters();
    }
    
    internal static string ReplaceForbiddenRowKeyParameters(this NoEmptyString value)
    {
        return value.ToString().ReplaceForbiddenRowKeyParameters();
    }

    internal static string ReplaceForbiddenRowKeyParameters(this string value)
    {
        return ReplacementCharacters.Aggregate(value,
            (current, item) => current.Replace(item.Key.ToString(), item.Value));
    }

    internal static string RestoreForbiddenRowKeyParameters(this string value)
    {
        return ReplacementCharacters.Aggregate(value,
            (current, item) => current.Replace(item.Value, item.Key.ToString()));
    }
}