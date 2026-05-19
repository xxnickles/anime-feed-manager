using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Cosmos.Types;

namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Abstract base for every anime series document. Polymorphic via STJ
/// <see cref="JsonPolymorphicAttribute"/>; one Cosmos container at <c>/season</c>.
/// Concrete derived types declare their type-specific surface; reads come back
/// as the right concrete type by the <c>seriesType</c> discriminator.
/// </summary>
[CosmosEntity("series", "/season")]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "seriesType")]
[JsonDerivedType(typeof(TvSeries), "tv")]
[JsonDerivedType(typeof(MovieSeries), "movie")]
[JsonDerivedType(typeof(OvaSeries), "ova")]
[JsonDerivedType(typeof(OnaSeries), "ona")]
[JsonDerivedType(typeof(TvSpecialSeries), "tv_special")]
public abstract record Series : CosmosDocument
{
    public int MalId { get; }
    public string? MalUrl { get; init; }

    public Season Season { get; init; }
    public Year Year { get; init; }

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

    protected Series(int malId)
    {
        MalId = malId;
        Id = malId.ToString();
    }
}