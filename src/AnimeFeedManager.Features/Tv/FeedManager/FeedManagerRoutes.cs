namespace AnimeFeedManager.Features.Tv.FeedManager;

public static class FeedManagerRoutes
{
    /// <summary>Public catalog page of a nyaa-bound series; stored as its <c>FeedLink</c>.</summary>
    public static string Releases(string seriesId) => $"/tv/releases/{seriesId}";

    public static string AdminSeason(SeriesSeason season) => $"/admin/feeds/{season.ToSeasonKey()}";

    public static string AdminSeries(string seriesId) => $"/admin/feeds/series/{seriesId}";
}
