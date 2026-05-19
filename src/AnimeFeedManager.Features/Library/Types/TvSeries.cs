namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Broadcast TV series. Weekly schedule via <see cref="Broadcast"/> (nullable —
/// Jikan returns it sparsely for older shows).
/// </summary>
public sealed record TvSeries : Series
{
    public TvSeries(int malId) : base(malId)
    {
    }

    public Broadcast? Broadcast { get; init; }
    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}