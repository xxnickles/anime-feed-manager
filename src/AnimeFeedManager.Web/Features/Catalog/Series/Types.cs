using AnimeFeedManager.Shared.Types;

// Series (the entity) is fully qualified below: this namespace's own "Series" segment shadows the
// AnimeFeedManager.Features.Library.Entities.Series type for the unqualified name.
namespace AnimeFeedManager.Web.Features.Catalog.Series;

internal abstract record SeriesDetailContext;

internal record InvalidSeason() : SeriesDetailContext;
internal record SeriesNotFound() : SeriesDetailContext;
internal record ProcessFailed(): SeriesDetailContext;
internal record SeriesDetailAvailable(
    AnimeFeedManager.Features.Library.Entities.Series Series, SeriesSeason Season) : SeriesDetailContext;
