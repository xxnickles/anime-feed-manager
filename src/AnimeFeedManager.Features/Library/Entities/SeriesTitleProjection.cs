namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Lightweight cross-partition projection of a <see cref="Series"/> row — just the fields
/// needed to build a title-matching index, without paying for the full document.
/// <see cref="Season"/> rides along even though matching itself doesn't need it — it's free at
/// every construction site and turns a later by-id series lookup into a point-read instead of a
/// cross-partition query.
/// </summary>
public sealed record SeriesTitleProjection(int MalId, string[] AllTitles, SeriesSeason Season);
