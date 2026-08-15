using AnimeFeedManager.Features.Library.Airing.Types;

namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Single-document index of every TV series currently airing, sourced from Jikan
/// (<c>/v4/anime?status=airing&amp;type=tv</c>). Lives in the <c>system</c> Cosmos container,
/// sharing the <see cref="SystemDocument.SystemPartitionKey"/> partition with all other system
/// documents. Refreshed as a full replace, not a merge — a series absent from the latest pull is,
/// by construction, no longer airing, so "not present" is the removal signal. Replaces
/// <c>Series.Status</c>/<c>SeriesClassification.Trackability</c> as the source of truth for
/// "is this series airing right now."
/// </summary>
[CosmosEntity(CosmosContainers.System, "/partitionKey")]
public sealed record AiringSeriesIndex : SystemDocument
{
    public const string DocumentId = "airing-series-index";

    public ImmutableArray<AiringSeriesEntry> Entries { get; init; } = ImmutableArray<AiringSeriesEntry>.Empty;
}
