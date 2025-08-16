using System.Text.RegularExpressions;

namespace AnimeFeedManager.Web.BlazorComponents;

public static class UiUtils
{
    public static string GetUniqueName(string baseName) => $"{baseName}-{Guid.CreateVersion7().ToString("N")[..5]}";

    public static string PageTitle(string titleMessage) => $"Anime Feed Manager | {titleMessage}";
    public static string PageTitle() => PageTitle(string.Empty);

    public static string RemoveTrailingPeriod(this string message)
    {
        return message.EndsWith('.') ? message.TrimEnd('.') : message;
    }

    public static string RemoveAdditionalOccurrencesOf(this string input, string word)
    {
        // Do nothing if either the input or the word are null or empty
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(word))
            return input;

        if (!input.Contains(word, StringComparison.Ordinal))
        {
            return input; // Word not found
        }

        // Create a regex pattern to match the word followed by optional whitespace
        var pattern = $@"\b{Regex.Escape(word)}\b\s*";
        var matches = Regex.Matches(input, pattern);

        if (matches.Count <= 1)
        {
            return input; // Only one or no occurrence found
        }

        // Keep the first occurrence and remove the rest
        var result = input[..(matches[0].Index + matches[0].Length)];
        var remaining = input[(matches[0].Index + matches[0].Length)..];
        result += Regex.Replace(remaining, pattern, string.Empty);

        return result;
    }


    public static IReadOnlyDictionary<string, object> MergeAttributes(
        IReadOnlyDictionary<string, object>? additional,
        params (string Key, object Value)[] defaults)
    {
        var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        // seed defaults
        foreach (var (key, value) in defaults)
        {
            result[key] = value;
        }

        // overlay additional (parent-provided) values
        if (additional is null) return result;
        foreach (var pair in additional)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }
}