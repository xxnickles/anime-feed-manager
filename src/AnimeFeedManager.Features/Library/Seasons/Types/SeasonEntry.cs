namespace AnimeFeedManager.Features.Library.Seasons.Types;

/// <summary>
/// One entry per imported season. <paramref name="RepresentativePoster"/> is a
/// blob path (as <c>Series.CoverImageUrl</c>) for the archive card, always set by
/// import; <paramref name="IsCurrent"/> — set only by current-season imports,
/// single-valued across the index — marks the airing season.
/// </summary>
public sealed record SeasonEntry(
    SeriesSeason SeriesSeason,
    DateTimeOffset LastImportedAt,
    int SeriesCount,
    string RepresentativePoster,
    bool IsCurrent);