using System.Net;

namespace AnimeFeedManager.Features.Scrapping.Nyaa;

/// <summary>An HTML page in place of the feed: the DDoS-Guard challenge, which the client cannot pass.</summary>
internal sealed record NyaaChallengeError : DomainError
{
    private NyaaChallengeError(string message) : base(message)
    {
    }

    public static NyaaChallengeError Create(string request, HttpStatusCode status) =>
        new($"Nyaa answered {request} with an HTML page (status {(int) status}), likely a DDoS-Guard challenge");

    public override Action<ILogger> LogAction() =>
        logger => logger.LogError("{domain_error.message}", Message);
}
