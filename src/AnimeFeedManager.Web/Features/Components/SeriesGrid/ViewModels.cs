using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Web.Features.Components.SeriesGrid;

public record SeriesCardViewModel(
    string Title,
    string Synopsis,
    Uri? ImageUrl,
    DateTimeOffset? LastUpdated);


internal static class SeriesCardViewModelExtensions
{
    internal static SeriesCardViewModel AsSeriesCard(this TvSeries series) =>
        new(series.Title, series.Synopsis, series.Image, series.LastUpdated);
}