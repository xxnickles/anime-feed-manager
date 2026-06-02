namespace AnimeFeedManager.Features.Library.Images;

/// <summary>
/// Work-queue command requesting that a series' cover image be downloaded from
/// <paramref name="SourceUrl"/>, stored in blob storage, and the series'
/// <c>CoverImageUrl</c> patched to point at the stored blob path. Enqueued by the
/// library import after each successful series persist.
/// </summary>
/// <param name="Id">MAL id — the series' Cosmos document id.</param>
/// <param name="Season">The series' season — the Cosmos partition key and the blob path segment.</param>
/// <param name="SourceUrl">The Jikan cover URL to download (WebP preferred, JPG fallback).</param>
public sealed record ProcessSeriesImageCommand(string Id, SeriesSeason Season, string SourceUrl);
