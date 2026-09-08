namespace AnimeFeedManager.Features.Tv.Library.Storage;

/// <summary>
/// Alternative titles for a series, with the provider block and <paramref name="User"/> separately
/// owned: a scrap rewrites the provider slots, the editor rewrites <paramref name="User"/>, and
/// neither can resurrect titles the other deleted.
/// </summary>
public sealed record AlternativeTitlesData(
    string? Romaji = null,
    string? English = null,
    string? Native = null,
    string[]? Synonyms = null,
    string[]? User = null)
{
    public static readonly AlternativeTitlesData Empty = new();
}

// Its own context so absent slots stay out of the stored column, without changing how the
// feature's domain messages serialize.
[JsonSerializable(typeof(AlternativeTitlesData))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AlternativeTitlesJsonContext : JsonSerializerContext;

internal static class StoredAlternativeTitles
{
    /// <summary>Reads either the serialized object or the legacy '|'-separated list.</summary>
    internal static AlternativeTitlesData Parse(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
            return AlternativeTitlesData.Empty;

        if (!stored.TrimStart().StartsWith('{'))
            return FromLegacy(stored);

        try
        {
            return JsonSerializer.Deserialize(stored, AlternativeTitlesJsonContext.Default.AlternativeTitlesData)
                   ?? AlternativeTitlesData.Empty;
        }
        catch (JsonException)
        {
            return FromLegacy(stored);
        }
    }

    // Everything written before the object format was curated by hand.
    private static AlternativeTitlesData FromLegacy(string stored) => new(User: stored.StringToAppArray());

    extension(AlternativeTitlesData titles)
    {
        /// <summary>
        /// Titles worth matching a feed against, most specific first. Romaji repeats the canonical
        /// title and native never appears in a feed, so neither is offered here.
        /// </summary>
        internal string[] ForMatching
        {
            get
            {
                string?[] candidates = [.. titles.User ?? [], titles.English, .. titles.Synonyms ?? []];
                return candidates
                    .Where(title => !string.IsNullOrWhiteSpace(title))
                    .Select(title => title!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        internal bool IsEmpty =>
            string.IsNullOrWhiteSpace(titles.Romaji)
            && string.IsNullOrWhiteSpace(titles.English)
            && string.IsNullOrWhiteSpace(titles.Native)
            && titles.Synonyms is null or []
            && titles.User is null or [];

        internal string ToStoredString() =>
            titles.IsEmpty
                ? string.Empty
                : JsonSerializer.Serialize(titles, AlternativeTitlesJsonContext.Default.AlternativeTitlesData);
    }
}
