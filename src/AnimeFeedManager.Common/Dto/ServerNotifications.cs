namespace AnimeFeedManager.Common.Dto;
public record ConnectionInfo(string Url, string AccessToken);

public record HubInfo(string ConnectionId);
public static class ServerNotifications
{
    public const string TestNotification = "test";
}