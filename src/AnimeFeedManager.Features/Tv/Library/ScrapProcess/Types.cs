using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.SubsPlease;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public enum Status
{
    NewSeries,
    UpdatedSeries,
    NoChanges,
}

public sealed record StorageData(
    AnimeInfoStorage Series,
    ImageInformation Image,
    Status Status);

public sealed record ScrapTvLibraryData(
    IEnumerable<StorageData> SeriesData,
    ImmutableList<FeedData> FeedData,
    SeriesSeason Season);




