using System.Text.RegularExpressions;

namespace AnimeFeedManager.Shared;

public static partial class SharedUtils
{
    public static DateTime? ParseDate(string dateStr, int year)
    {
        const string pattern = @"(\d{1,2})(\w{2})(\s\w+)";
        const string replacement = "$1$3";
        var dateCleaned = Regex.Replace(dateStr, pattern, replacement);
        var result = DateTime.TryParse($"{dateCleaned} {year}", out var date);
        return result ? date : null;
    }

    public const char ArraySeparator = '|';

    /// <summary>
    /// Uses '|' as separator
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    public static string AppArrayToString(this string[] array) => string.Join(ArraySeparator, array);

    /// <summary>
    /// Assumes the array is separated by '|'
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string[] StringToAppArray(this string str) => str.Split(ArraySeparator,
        StringSplitOptions.RemoveEmptyEntries);


    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex CaseRegex();
}