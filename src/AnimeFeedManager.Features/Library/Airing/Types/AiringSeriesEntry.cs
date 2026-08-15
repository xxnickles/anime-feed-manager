namespace AnimeFeedManager.Features.Library.Airing.Types;

/// <summary>
/// One currently-airing TV series. <see cref="Season"/> matches the <c>Series</c> container's
/// partition-key format exactly, so a series can be located directly — no follow-up lookup.
/// <see cref="AllTitles"/> is denormalized from the same Jikan response (not re-fetched from
/// <c>Series</c>), so the TV title-matching index can be built straight from this entry.
/// </summary>
public sealed record AiringSeriesEntry(int MalId, SeriesSeason Season, string[] AllTitles);
