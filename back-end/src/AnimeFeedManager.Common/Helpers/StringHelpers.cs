using System.Text.RegularExpressions;

namespace AnimeFeedManager.Common.Helpers
{
    public static class StringHelpers
    {
        public static string SplitWordsByCase(string str)
        {
            var split = Regex.Split(str, @"(?<!^)(?=[A-Z])");
            return string.Join(' ', split);
        }
    }
}
