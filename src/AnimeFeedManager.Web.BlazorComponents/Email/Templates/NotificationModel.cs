namespace AnimeFeedManager.Web.BlazorComponents.Email.Templates;

/// <summary>
/// Model for anime feed notification email
/// </summary>
public record NotificationModel(
    string UserName,
    string UserEmail,
    AnimeFeedGroup[] AnimeFeeds)
{
    /// <summary>
    /// Total count of all episodes across all anime
    /// </summary>
    public int TotalEpisodeCount => AnimeFeeds.Sum(a => a.Episodes.Length);
}

/// <summary>
/// Represents a group of episodes for a single anime series
/// </summary>
public record AnimeFeedGroup(
    string Title,
    string Url,
    EpisodeInfo[] Episodes);

/// <summary>
/// Information about a single episode
/// </summary>
public record EpisodeInfo(
    string EpisodeNumber,
    string MagnetLink,
    string TorrentLink,
    bool IsNew);
