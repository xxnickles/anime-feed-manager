namespace AnimeFeedManager.Web.BlazorComponents.Tests.Helpers;

internal static class EmailModels
{
    public static NotificationModel ForEpisodes(params string[] episodeNumbers) =>
        Notification(Series("Some Series", episodeNumbers));

    public static NotificationModel Notification(params AnimeFeedGroup[] series) =>
        new("Tester", "tester@example.com", series);

    public static AnimeFeedGroup Series(string title, params string[] episodeNumbers) =>
        new(title, "https://example.com/series", episodeNumbers.Select(Episode).ToArray());

    public static EpisodeInfo Episode(string episodeNumber) =>
        new(episodeNumber,
            $"magnet:?xt=urn:btih:{episodeNumber}",
            $"https://example.com/torrent/{episodeNumber}",
            true);
}
