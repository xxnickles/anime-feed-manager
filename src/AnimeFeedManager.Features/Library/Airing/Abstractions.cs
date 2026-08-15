using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Library.Airing;

/// <summary>Point-read of the single <see cref="AiringSeriesIndex"/> document. Absence reads as an empty index, not an error.</summary>
public delegate Task<Result<AiringSeriesIndex>> AiringSeriesIndexLoader(CancellationToken cancellationToken = default);

/// <summary>Overwrites the index wholesale with <paramref name="entries"/> — full replace, not a merge.</summary>
public delegate Task<Result<Unit>> AiringSeriesIndexReplacer(
    ImmutableArray<AiringSeriesEntry> entries, CancellationToken cancellationToken = default);
