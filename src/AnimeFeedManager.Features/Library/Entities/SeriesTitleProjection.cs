namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Lightweight cross-partition projection of a <see cref="Series"/> row — just the fields
/// needed to build a title-matching index, without paying for the full document.
/// </summary>
public sealed record SeriesTitleProjection(int MalId, string[] AllTitles);
