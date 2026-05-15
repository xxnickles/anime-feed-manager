namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

/// <summary>
/// A single page of Jikan results yielded by <see cref="IJikanClient"/>.
/// Carries the page's anime payload plus pagination context so consumers
/// can emit progress (e.g. "page 3 of 7") without re-reading the envelope.
/// </summary>
public sealed record JikanPage(
    ImmutableArray<JikanAnime> Items,
    int Page,
    int LastPage,
    int TotalItems);
