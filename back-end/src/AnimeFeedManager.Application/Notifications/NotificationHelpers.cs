namespace AnimeFeedManager.Application.Notifications;

internal static class NotificationHelpers
{
    internal static string FormatMagnetLink(string link)
    {
        return link.Replace("&", "&amp;");
    }
}