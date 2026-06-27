using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Shared.Types;

namespace AnimeFeedManager.Web.Features.Catalog;

internal abstract record SeriesDetailContext;

internal record InvalidSeason() : SeriesDetailContext;
internal record SeriesNotFound() : SeriesDetailContext;
internal record ProcessFailed(): SeriesDetailContext;
internal record SeriesDetailAvailable(Series Series, SeriesSeason Season) : SeriesDetailContext;
