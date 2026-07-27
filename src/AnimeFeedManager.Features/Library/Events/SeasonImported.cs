namespace AnimeFeedManager.Features.Library.Events;

/// <summary>
/// Raised once a library import completes successfully with at least one series persisted.
/// Fed to the SSE notification channels (a public "new season available" toast and an admin
/// operational toast) and, via <c>SeasonImportedEventHandler</c>, persisted as a
/// <c>LibraryEvent</c> for the admin activity feed — not purely fire-and-forget.
/// <see cref="CoverPath"/> is the best-scored stored cover's blob path (null when no cover was
/// stored); the render side prefixes the blob base. <see cref="ByType"/> breaks the imported count
/// down by series type (TV, Movie, OVA, ...); the total is its sum, so it isn't carried separately.
/// </summary>
public sealed record SeasonImported(
    SeriesSeason Season,
    string? CoverPath,
    ImmutableArray<SeriesTypeCount> ByType,
    DateTimeOffset OccurredAt);

/// <summary>Count of successfully persisted series of a given <see cref="Series.TypeKey"/>/<see cref="Series.TypeLabel"/>.</summary>
public readonly record struct SeriesTypeCount(string TypeKey, string TypeLabel, int Count);
