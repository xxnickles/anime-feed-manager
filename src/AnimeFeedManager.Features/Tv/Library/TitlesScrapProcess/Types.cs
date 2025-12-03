using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

public enum UpdateStatus
{
    Updated,
    NoChanges
}

public sealed record FeedTitleUpdateInformation(AnimeInfoStorage Series, UpdateStatus UpdateStatus);

public sealed record FeedTitleUpdateData(
    SeriesSeason Season,
    ImmutableList<FeedData> FeedData,
    ImmutableList<FeedTitleUpdateInformation> FeedTitleUpdateInformation);