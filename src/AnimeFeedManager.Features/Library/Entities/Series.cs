using static AnimeFeedManager.Features.Library.Entities.Constants;

namespace AnimeFeedManager.Features.Library.Entities;

/// <summary>
/// Abstract base for every anime series document. Polymorphic via STJ
/// <see cref="JsonPolymorphicAttribute"/>; one Cosmos container at <c>/season</c>.
/// Concrete derived types declare their type-specific surface; reads come back
/// as the right concrete type by the <c>seriesType</c> discriminator.
/// </summary>
[CosmosEntity(CosmosContainers.Series, SeriesPartitionKey)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "seriesType")]
[JsonDerivedType(typeof(TvSeries), "tv")]
[JsonDerivedType(typeof(MovieSeries), "movie")]
[JsonDerivedType(typeof(OvaSeries), "ova")]
[JsonDerivedType(typeof(OnaSeries), "ona")]
[JsonDerivedType(typeof(TvSpecialSeries), "tv_special")]
[JsonDerivedType(typeof(SpecialSeries), "special")]
public abstract record Series : CosmosDocument
{
    public int MalId { get; }
    public string? MalUrl { get; init; }

    public SeriesSeason SeriesSeason { get; init; } = SeriesSeason.Default;

    public SeriesTitles Titles { get; init; }
    public string[] AllTitles { get; init; } = [];

    public string? Synopsis { get; init; }
    public string? CoverImageUrl { get; init; }
    public string? TrailerUrl { get; init; }

    public SeriesStatus Status { get; init; } = SeriesStatus.Unknown();
    public string[] Genres { get; init; } = [];
    public string[] Studios { get; init; } = [];
    public double? Score { get; init; }
    public string? Source { get; init; }

    public DateTimeOffset? AiredFrom { get; init; }
    public DateTimeOffset? AiredTo { get; init; }

    public DateTimeOffset LastUpdated { get; init; }

    /// <summary>Stable type key — mirrors the JSON <c>seriesType</c> discriminator (e.g. "tv", "movie").</summary>
    public abstract string TypeKey { get; }

    /// <summary>Human label for the series format (e.g. "TV", "Movie").</summary>
    public abstract string TypeLabel { get; }

    /// <summary>Broadcast schedule when the format has one (TV only); <c>null</c> otherwise.</summary>
    public virtual Broadcast? Schedule => null;

    /// <summary>One-line format detail for cards/listings — episode count or runtime; <c>null</c> when unknown.</summary>
    public virtual string? FormatSummary => null;

    /// <summary>Shared formatter for the episode-count summary used by every episodic format.</summary>
    protected static string? EpisodeSummary(int? episodes) => episodes is { } count ? $"{count} ep" : null;

    protected Series(int malId)
    {
        MalId = malId;
        Id = malId.ToString();
    }
}