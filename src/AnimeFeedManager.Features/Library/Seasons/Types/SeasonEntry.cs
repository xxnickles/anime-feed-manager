namespace AnimeFeedManager.Features.Library.Seasons.Types;

/// <summary>
/// One entry per imported season — the typed <see cref="SeriesSeason"/>, when
/// the season was last imported, and how many series the import produced.
/// </summary>
public sealed record SeasonEntry(
    SeriesSeason SeriesSeason,
    DateTimeOffset LastImportedAt,
    int SeriesCount);