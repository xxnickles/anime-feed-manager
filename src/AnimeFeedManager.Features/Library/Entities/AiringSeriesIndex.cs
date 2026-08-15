using AnimeFeedManager.Features.Library.Airing.Types;

namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Single-document index of every currently-airing TV series, sourced from Jikan
/// (<c>/v4/anime?status=airing&amp;type=tv</c>). Refreshed as a full replace — absence from the
/// latest pull means "no longer airing", so no separate removal step is needed.
/// </summary>
[CosmosEntity(CosmosContainers.System, "/partitionKey")]
public sealed record AiringSeriesIndex : SystemDocument
{
    public const string DocumentId = "airing-series-index";

    public ImmutableArray<AiringSeriesEntry> Entries { get; init; } = ImmutableArray<AiringSeriesEntry>.Empty;
}
