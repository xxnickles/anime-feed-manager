namespace AnimeFeedManager.Features.Library.Airing.Types;

/// <summary>
/// One currently-airing TV series. <see cref="Season"/> matches <c>Series</c>'s partition-key
/// format directly. <see cref="AllTitles"/> is denormalized from Jikan so a title-matching index
/// can build straight from this entry, no <c>Series</c> lookup needed.
/// </summary>
public sealed record AiringSeriesEntry(int MalId, SeriesSeason Season, string[] AllTitles);
