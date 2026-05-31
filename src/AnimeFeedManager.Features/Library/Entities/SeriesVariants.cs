namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Broadcast TV series. Weekly schedule via <see cref="Broadcast"/> (nullable —
/// Jikan returns it sparsely for older shows).
/// </summary>
public sealed record TvSeries : Series
{
    public TvSeries(int malId) : base(malId) { }

    public Broadcast? Broadcast { get; init; }
    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}

/// <summary>
/// Theatrical anime film — single release date, single total runtime.
/// </summary>
public sealed record MovieSeries : Series
{
    public MovieSeries(int malId) : base(malId) { }

    public int? RuntimeMinutes { get; init; }
}

/// <summary>
/// Original Video Animation — direct-to-video/Blu-ray episodic release.
/// No broadcast schedule; volumes drop over an air range.
/// </summary>
public sealed record OvaSeries : Series
{
    public OvaSeries(int malId) : base(malId) { }

    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}

/// <summary>
/// Original Net Animation — streaming-native release. Episodic; no broadcast
/// schedule on the seasonal feed (verified TV-only), so episodes drop over an air range.
/// </summary>
public sealed record OnaSeries : Series
{
    public OnaSeries(int malId) : base(malId) { }

    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}

/// <summary>
/// TV special — short-form broadcast tied to a parent series. Episodic per Jikan
/// (multiple short episodes are common), so modeled like <see cref="OvaSeries"/>.
/// </summary>
public sealed record TvSpecialSeries : Series
{
    public TvSpecialSeries(int malId) : base(malId) { }

    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}

/// <summary>
/// Standalone special / extra (distinct MAL type from <see cref="TvSpecialSeries"/> —
/// e.g. bundled BD/DVD extras). Episodic shape; kept as its own entity to preserve
/// MAL's upstream type distinction even though its fields match a TV special.
/// </summary>
public sealed record SpecialSeries : Series
{
    public SpecialSeries(int malId) : base(malId) { }

    public int? Episodes { get; init; }
    public int? EpisodeDurationMinutes { get; init; }
}

/// <summary>
/// Structured title set for a series. <see cref="Default"/> is the canonical title;
/// the others may be missing depending on Jikan's data per record. For lookup/feed
/// matching, use the flattened <c>Series.AllTitles</c> projection.
/// </summary>
public readonly record struct SeriesTitles(
    string Default,
    string? English,
    string? Japanese,
    string[] Synonyms);

/// <summary>
/// Broadcast schedule for episodically-aired series. Stored as raw Jikan strings;
/// values like <c>"Saturdays"</c>, <c>"23:30"</c>, <c>"Asia/Tokyo"</c>.
/// </summary>
public readonly record struct Broadcast(string Day, string Time, string Timezone);
