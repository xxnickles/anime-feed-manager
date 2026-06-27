using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Library.Import.Jikan.Parsing;

internal static class JikanParsers
{
    /// <summary>
    /// Validates the one hard requirement that survives type filtering and external season
    /// resolution: a positive MAL id. Season/year are no longer checked here — they're
    /// resolved once by the client (TV-only on Jikan) and passed into the mapper.
    /// </summary>
    internal static Validation<Unit> ValidateRequired(JikanAnime jikan) =>
        jikan.MalId > 0
            ? Validation<Unit>.Valid(new Unit())
            : Validation<Unit>.Invalid($"mal_id must be positive (got {jikan.MalId})");
}
