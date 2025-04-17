using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Tv.Types;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.Types;

public readonly record struct TvSeries(ImmutableList<AnimeInfoStorage> SeriesList, ImmutableList<DownloadImageEvent> Images);
public readonly record struct TilesMap(string Original, string Alternative);