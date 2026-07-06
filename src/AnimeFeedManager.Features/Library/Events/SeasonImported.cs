using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Features.Library.Events;

/// <summary>
/// Raised once a library import completes successfully with at least one series persisted.
/// A fire-and-forget domain event fed to the SSE notification channels: a public "new season
/// available" toast and an admin operational toast. <see cref="CoverPath"/> is the best-scored
/// stored cover's blob path (null when no cover was stored); the render side prefixes the blob base.
/// </summary>
public sealed record SeasonImported(SeriesSeason Season, int Imported, string? CoverPath);
