namespace AnimeFeedManager.Features.Tv.Library.Storage;

/// <summary>Provider alternative titles for a series; every import rewrites them.</summary>
public sealed record AlternativeTitlesData(
    string? Romaji = null,
    string? English = null,
    string? Native = null,
    string[]? Synonyms = null,
    string? Abbreviation = null)
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
    /// <summary>Reads the serialized object; anything else reads as empty until the next import rewrites it.</summary>
    internal static AlternativeTitlesData Parse(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored) || !stored.TrimStart().StartsWith('{'))
            return AlternativeTitlesData.Empty;

        try
        {
            return JsonSerializer.Deserialize(stored, AlternativeTitlesJsonContext.Default.AlternativeTitlesData)
                   ?? AlternativeTitlesData.Empty;
        }
        catch (JsonException)
        {
            return AlternativeTitlesData.Empty;
        }
    }

    extension(AlternativeTitlesData titles)
    {
        /// <summary>
        /// Titles worth matching a feed against, most specific first. Romaji repeats the canonical
        /// title, native never appears in a feed and abbreviations are too loose, so none is offered here.
        /// </summary>
        internal string[] ForMatching
        {
            get
            {
                string?[] candidates = [titles.English, .. titles.Synonyms ?? []];
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
            && string.IsNullOrWhiteSpace(titles.Abbreviation);

        internal string ToStoredString() =>
            titles.IsEmpty
                ? string.Empty
                : JsonSerializer.Serialize(titles, AlternativeTitlesJsonContext.Default.AlternativeTitlesData);
    }
}
