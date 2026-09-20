using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public enum Status
{
    NewSeries,
    UpdatedSeries,
    NoChanges,
}

/// <summary>
/// The provider's airing state for a series, as reported during import. Separate from the stored
/// <c>SeriesStatus</c>: providers distinguish states we do not persist, and the value set is open,
/// so anything unrecognised — including a provider that reports nothing — lands on
/// <see cref="Unknown"/> and the feed decides.
/// </summary>
public enum AiringStatus
{
    Unknown,
    Upcoming,
    Ongoing,
    Finished,
    Delayed
}

public sealed record StorageData(
    AnimeInfoStorage Series,
    ImageInformation Image,
    Status Status,
    AiringStatus Airing);

public sealed record ScrapTvLibraryData(
    IEnumerable<StorageData> SeriesData,
    ImmutableArray<FeedData> FeedData,
    SeriesSeason Season);




