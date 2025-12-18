using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Web.BlazorComponents.Email.Templates;

/// <summary>
/// Maps domain events to email template models
/// </summary>
public static class AnimeFeedNotificationMapper
{
    /// <summary>
    /// Converts a FeedNotification domain event to an email template model
    /// </summary>
    /// <param name="notification">The feed notification event</param>
    /// <returns>Email template model</returns>
    public static NotificationModel ToEmailModel(
        this FeedNotification notification)
    {
        var animeFeeds = notification.Feeds
            .Select(feed => new AnimeFeedGroup(
                Title: feed.Title,
                Url: feed.Url,
                Episodes: feed.Episodes
                    .Select(ep => new EpisodeInfo(
                        EpisodeNumber: ep.EpisodeNumber,
                        MagnetLink: ep.MagnetLink,
                        TorrentLink: ep.TorrentLink,
                        IsNew: ep.IsNew))
                    .ToArray()))
            .ToArray();

        // Extract user name from email (e.g., "john.doe@example.com" -> "John Doe")
        var userName = ExtractUserNameFromEmail(notification.Subscriptions.UserEmail);

        return new NotificationModel(
            UserName: userName,
            UserEmail: notification.Subscriptions.UserEmail,
            AnimeFeeds: animeFeeds);
    }

    private static string ExtractUserNameFromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "User";

        var localPart = email.Split('@')[0];

        // Replace common separators with spaces and title case
        var name = localPart
            .Replace('.', ' ')
            .Replace('_', ' ')
            .Replace('-', ' ');

        // Title case each word
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var titleCased = words.Select(word =>
            char.ToUpper(word[0]) + word[1..].ToLower());

        return string.Join(' ', titleCased);
    }
}
