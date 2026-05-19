namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Structured title set for a series. <see cref="Default"/> is the canonical title;
/// the others may be missing depending on Jikan's data per record. For lookup/feed
/// matching, use the flattened <c>Series.AllTitles</c> projection.
/// </summary>
public readonly record struct SeriesTitles(
    string Default,
    string? English,
    string? Japanese,
    string[] Synonyms);