namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

/// <summary>
/// Source of truth for the adult-content markers we exclude at import. Jikan segregates
/// hentai into <c>explicit_genres</c> (it never appears in the regular <c>genres</c> array
/// the mapper reads) and stamps the corresponding <c>rating</c> as "Rx - Hentai"; matching
/// either signal is enough. Titles flagged here are dropped at the client boundary and never
/// reach the mapper or Cosmos. Erotica and other explicit genres are intentionally left in.
/// </summary>
internal static class JikanAdultContent
{
    /// <summary>MAL's stable genre id for Hentai (https://myanimelist.net/anime/genre/12).</summary>
    private const int HentaiGenreId = 12;

    /// <summary>The sole "Rx" rating value MAL emits, reserved for hentai.</summary>
    private const string HentaiRating = "Rx - Hentai";

    /// <summary>True if <paramref name="anime"/> is hentai by explicit genre or rating.</summary>
    public static bool IsHentai(JikanAnime anime) =>
        anime.ExplicitGenres.Any(genre => genre.MalId == HentaiGenreId) ||
        string.Equals(anime.Rating, HentaiRating, StringComparison.Ordinal);
}
