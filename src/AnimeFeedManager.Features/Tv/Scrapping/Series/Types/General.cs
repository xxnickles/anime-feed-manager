using AnimeFeedManager.Features.Tv.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.Types
{
    public readonly record struct TvSeries(ImmutableList<AnimeInfoStorage> SeriesList, ImmutableList<DownloadImageEvent> Images);
}