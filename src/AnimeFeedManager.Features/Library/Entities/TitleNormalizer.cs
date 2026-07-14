using System.Text.RegularExpressions;

namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Collapses a title to lowercase alphanumerics only, dropping spaces/punctuation entirely.
/// Different fansub groups segment the same title differently ("Shite mo" vs "shitemo",
/// "EXCEEDS - Gun Blaze Vengeance" vs "Exceeds: Gun Blaze Vengeance") — squashing everything
/// non-alphanumeric makes these compare equal without a full fuzzy-matching library.
/// </summary>
internal static partial class TitleNormalizer
{
    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphanumeric();

    public static string Normalize(string title) => NonAlphanumeric().Replace(title, "").ToLowerInvariant();
}
