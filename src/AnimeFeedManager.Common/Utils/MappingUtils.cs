﻿using System.Text.RegularExpressions;

namespace AnimeFeedManager.Common.Utils;

public static class MappingUtils
{
    public static DateTime? ParseDate(string dateStr, int year)
    {
        const string pattern = @"(\d{1,2})(\w{2})(\s\w+)";
        const string replacement = "$1$3";
        var dateCleaned = Regex.Replace(dateStr, pattern, replacement);
        var result = DateTime.TryParse($"{dateCleaned} {year}", out var date);
        return result ? date : null;
    }
}