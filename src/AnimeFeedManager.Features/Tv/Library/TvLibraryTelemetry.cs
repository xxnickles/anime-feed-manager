namespace AnimeFeedManager.Features.Tv.Library;

public static class TvLibraryTelemetry
{
    public const string ImportSpan = "Tv.Library.Import";
    public const string UpdateFeedTitlesSpan = "Tv.Library.UpdateFeedTitles";
    public const string ScrapFeedTitlesSpan = "Tv.Library.ScrapFeedTitles";
    public const string CompleteOngoingSpan = "Tv.Library.CompleteOngoing";
    public const string QuerySeasonSpan = "Tv.Library.QuerySeason";

    public const string SeriesCountTag = "series.count";
    public const string FeedTitleCountTag = "feed.title_count";
}
