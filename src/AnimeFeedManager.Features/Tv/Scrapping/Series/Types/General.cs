using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.Types
{
    public readonly record struct TvSeries(ImmutableList<AnimeInfoStorage> SeriesList, ImmutableList<DownloadImageEvent> Images);
}