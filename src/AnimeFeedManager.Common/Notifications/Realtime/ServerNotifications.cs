
namespace AnimeFeedManager.Common.Notifications.Realtime;
public record ConnectionInfo(string Url, string AccessToken);

public record HubInfo(string ConnectionId);

public static class ServerNotifications
{
    public const string SeasonProcess = "seasonprocess";
    public const string TitleUpdate = "titleupdate";
    public const string ImageUpdate = "imageupdate";
}