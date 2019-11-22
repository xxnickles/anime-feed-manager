using System.Text.RegularExpressions;

namespace AnimeFeedManager.Common.Extensions
{
    public static class IdHelpers
    {
        public static string ToApplicationId(this string @this)
        {
            var noSpecialCharactersString = Regex.Replace(@this, "[^a-zA-Z0-9_.\\s]+", "", RegexOptions.Compiled);
            return  noSpecialCharactersString
                .Replace(" ", "_")
                .Replace("__", "_");
        }
    }
}
