using AnimeFeedManager.Features.Common.Scrapping;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public sealed record StorageData(
    AnimeInfoStorage Series,
    ImageInformation Image);

public sealed record ScrapTvLibraryData(
    IEnumerable<StorageData> SeriesData,
    ImmutableList<string> FeedTitles,
    SeriesSeason Season);




