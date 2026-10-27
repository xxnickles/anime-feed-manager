namespace AnimeFeedManager.Features.Tv.Library.Storage.Stores;

public abstract record SeriesFeed;

public sealed record NoFeed : SeriesFeed;

public sealed record SubsPleaseFeed(string Title, string Link) : SeriesFeed;

/// <summary>Fed by a manual nyaa binding; takes priority over any SubsPlease match.</summary>
public sealed record LocalFeed(string Title, string Link) : SeriesFeed;

public static class SeriesFeeds
{
    public const string SubsPleaseSource = "SubsPlease";
    public const string LocalSource = "Local";

    extension(AnimeInfoStorage series)
    {
        /// <summary>A feed title with no source is a legacy SubsPlease row.</summary>
        public SeriesFeed Feed => series switch
        {
            {FeedSource: LocalSource} => new LocalFeed(series.FeedTitle ?? string.Empty, series.FeedLink ?? string.Empty),
            {FeedTitle: var title} when string.IsNullOrWhiteSpace(title) => new NoFeed(),
            _ => new SubsPleaseFeed(series.FeedTitle!, series.FeedLink ?? string.Empty)
        };
    }
}
