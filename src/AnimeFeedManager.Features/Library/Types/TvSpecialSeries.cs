namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// TV special — one-shot or short-form broadcast tied to a parent series.
/// Single air date, single total runtime.
/// </summary>
public sealed record TvSpecialSeries : Series
{
    public TvSpecialSeries(int malId) : base(malId) { }

    public int? RuntimeMinutes { get; init; }
}
