namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

/// <summary>
/// A single page of the currently-airing-TV bulk fetch. Unlike <see cref="JikanPage"/>, there's
/// no single page-level season to resolve — <see cref="JikanAnime.Season"/>/<see cref="JikanAnime.Year"/>
/// are read per item, since this fetch spans every season a TV series could have premiered in.
/// </summary>
public sealed record JikanAiringPage(
    ImmutableArray<JikanAnime> Items,
    int Page,
    int LastPage)
{
    /// <summary>True when this page is empty because Jikan reported itself unavailable (504) — see <see cref="JikanUnavailableError"/>.</summary>
    public bool Degraded { get; init; }
}
