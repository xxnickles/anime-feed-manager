namespace AnimeFeedManager.Features.Scrapping.Types;

// Used for configuration - supports both local Chrome path and remote WebSocket endpoint
public sealed record PuppeteerOptions(
    string? LocalPath = null,
    string? RemoteEndpoint = null,
    string Token = "",
    bool RunHeadless = true)
{
    public bool UseRemote => !string.IsNullOrEmpty(RemoteEndpoint);
}
