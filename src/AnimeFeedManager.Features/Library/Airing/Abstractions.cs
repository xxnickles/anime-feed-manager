using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Library.Airing;

/// <summary>Point-read of the single <see cref="AiringSeriesIndex"/> document. Absence reads as an empty index, not an error.</summary>
public delegate Task<Result<AiringSeriesIndex>> AiringSeriesIndexLoader(CancellationToken cancellationToken = default);

/// <summary>
/// Overwrites the index wholesale with <paramref name="entries"/> — a full replace, not a merge.
/// A series absent from the new set is dropped; there's no separate removal step.
/// </summary>
public delegate Task<Result<Unit>> AiringSeriesIndexReplacer(
    ImmutableArray<AiringSeriesEntry> entries, CancellationToken cancellationToken = default);
