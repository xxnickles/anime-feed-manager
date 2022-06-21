namespace AnimeFeedManager.Application.Notifications;

internal class NotificationHelpers
{
    internal static string FormatMagnetLink(string link)
    {
        return link.Replace("&", "&amp;");
    }
}