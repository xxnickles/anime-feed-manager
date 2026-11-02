namespace AnimeFeedManager.Features.Common;

/// <summary>Public web app origin, for links that leave the app (emails).</summary>
public sealed record WebAppHost(Uri BaseUrl)
{
    public string Absolute(string path) => new Uri(BaseUrl, path).ToString();
}
