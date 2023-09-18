
namespace AnimeFeedManager.Features.Common.RealTimeNotifications;

public record ConnectionInfo(string Url, string AccessToken);

public static class ServerNotifications
{
    public const string SeasonProcess = "seasonprocess";
    public const string TitleUpdate = "titleupdate";
    public const string ImageUpdate = "imageupdate";
}