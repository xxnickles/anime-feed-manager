using AnimeFeedManager.Features.Domain;

namespace AnimeFeedManager.Features.Tv.Scrapping.Types;

public readonly record struct TvSeries(ImmutableList<AnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);