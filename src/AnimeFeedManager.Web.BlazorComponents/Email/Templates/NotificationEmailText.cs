using System.Text;

namespace AnimeFeedManager.Web.BlazorComponents.Email.Templates;

/// <summary>
/// Plain-text alternative for <see cref="NotificationEmail" />. Mirrors its content and ordering.
/// </summary>
public static class NotificationEmailText
{
    private const string Divider = "----------------------------------------";

    public static string Render(NotificationModel model)
    {
        var header = new[]
        {
            $"NEW EPISODES - {DateTime.Today:MMMM dd, yyyy}",
            Divider,
            string.Empty,
            $"Hi {model.UserName},",
            string.Empty,
            $"You have {model.TotalEpisodeCount} new {Pluralize("episode", model.TotalEpisodeCount)} waiting from your subscriptions.",
            string.Empty
        };

        var series = model.AnimeFeeds.Select(RenderSeries);

        var footer = new[]
        {
            Divider,
            "You're receiving this because you subscribed to these series.",
            $"Anime Feed Manager (c) {DateTime.Today.Year}"
        };

        return string.Join(Environment.NewLine, header.Concat(series).Concat(footer));
    }

    private static string RenderSeries(AnimeFeedGroup anime)
    {
        var lines = new[]
        {
            anime.Title,
            $"  {anime.Episodes.Length} {Pluralize("episode", anime.Episodes.Length)} - view on SubsPlease:",
            $"  {anime.Url}",
            string.Empty
        };

        // Same ordering as the HTML template so both parts agree.
        var episodes = anime.Episodes
            .OrderByDescending(episode => episode.EpisodeNumber)
            .Select(RenderEpisode);

        return string.Join(Environment.NewLine, lines.Concat(episodes));
    }

    private static string RenderEpisode(EpisodeInfo episode)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"  EP {episode.EpisodeNumber}{(episode.IsNew ? "  [NEW]" : string.Empty)}");
        builder.AppendLine($"    Magnet:  {episode.MagnetLink}");
        builder.AppendLine($"    Torrent: {episode.TorrentLink}");
        return builder.ToString();
    }

    private static string Pluralize(string word, int count) => count == 1 ? word : $"{word}s";
}
