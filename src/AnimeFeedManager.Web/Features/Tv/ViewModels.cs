using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Web.Features.Tv;

internal abstract record TvSeriesViewModel(TvSeries TvSeries);

internal record NoAvailableSeriesViewModel(TvSeries TvSeries) : TvSeriesViewModel(TvSeries);

internal record AvailableSeriesViewModel(TvSeries TvSeries) : TvSeriesViewModel(TvSeries);