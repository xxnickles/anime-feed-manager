using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Library.Catalog;

public abstract class CatalogQueries
{
    public static Task<Result<ImmutableArray<Series>>> GetSeasonalCatalog(
        SeriesBySeasonLoader loader,
        SeriesSeason season,
        CancellationToken cancellationToken)
    {
        return loader(season, cancellationToken)
            .WithLogProperty(nameof(SeriesSeason), season);
    }

    public static Task<Result<Series>> GetSeasonalSeries(
        SeriesByIdLoader loader,
        SeriesSeason season, int malId,
        CancellationToken cancellationToken)
    {
        return loader(season, malId, cancellationToken)
            .WithLogProperty(nameof(SeriesSeason), season);
    }
}