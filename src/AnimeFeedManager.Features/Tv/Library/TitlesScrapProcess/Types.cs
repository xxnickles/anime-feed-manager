namespace AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

public enum UpdateStatus
{
    Updated,
    NoChanges
}

public sealed record FeedTitleUpdateInformation(AnimeInfoStorage Series, UpdateStatus UpdateStatus);

public sealed record FeedTitleUpdateData(
    SeriesSeason Season,
    ImmutableList<string> FeedTitles,
    ImmutableList<FeedTitleUpdateInformation> FeedTitleUpdateInformation);