using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

/// <summary>
/// The single source of truth for Jikan's <c>type</c> field values we care about.
/// Used as constant patterns in the mapper's type dispatch and as the import whitelist
/// in the client. Any value not in <see cref="IsModeled"/> (Music, PV, CM, null, …) is
/// filtered out at the client boundary and never reaches the mapper.
/// </summary>
internal static class JikanAnimeType
{
    public const string Tv = "TV";
    public const string Movie = "Movie";
    public const string Ova = "OVA";
    public const string Ona = "ONA";
    public const string TvSpecial = "TV Special";
    public const string Special = "Special";

    private static readonly FrozenSet<string> Modeled =
        FrozenSet.ToFrozenSet([Tv, Movie, Ova, Ona, TvSpecial, Special]);

    /// <summary>True if <paramref name="type"/> is a series type we model and import.</summary>
    public static bool IsModeled([NotNullWhen(true)] string? type) =>
        type is not null && Modeled.Contains(type);
}
